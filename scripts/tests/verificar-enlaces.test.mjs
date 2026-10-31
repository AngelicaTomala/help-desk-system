// Pruebas de scripts/verificar-enlaces.mjs: node --test "scripts/tests/*.test.mjs"

import { after, describe, test } from 'node:test';
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { calcularSlug, normalizarFinesDeLinea, verificar } from '../verificar-enlaces.mjs';

const carpetaPruebas = path.dirname(fileURLToPath(import.meta.url));
const fixtures = path.join(carpetaPruebas, 'fixtures');
const script = path.join(carpetaPruebas, '..', 'verificar-enlaces.mjs');
const raizRepositorio = path.join(carpetaPruebas, '..', '..');
const temporal = fs.mkdtempSync(path.join(os.tmpdir(), 'verificar-enlaces-'));

after(() => fs.rmSync(temporal, { recursive: true, force: true }));

// Git puede entregar los ejemplos con CRLF (core.autocrlf), así que se copian con un fin de línea
// fijo a una carpeta temporal.
function copiarConFinDeLinea(destino, finDeLinea, filtro = () => true) {
  const copiar = (origen, carpeta) => {
    fs.mkdirSync(carpeta, { recursive: true });
    for (const entrada of fs.readdirSync(origen, { withFileTypes: true })) {
      const rutaOrigen = path.join(origen, entrada.name);
      const rutaDestino = path.join(carpeta, entrada.name);
      if (entrada.isDirectory()) {
        copiar(rutaOrigen, rutaDestino);
      } else if (filtro(path.relative(fixtures, rutaOrigen))) {
        const texto = normalizarFinesDeLinea(fs.readFileSync(rutaOrigen, 'utf8'));
        fs.writeFileSync(rutaDestino, texto.replace(/\n/g, finDeLinea));
      }
    }
  };
  copiar(fixtures, destino);
  return destino;
}

const variantes = {
  LF: copiarConFinDeLinea(path.join(temporal, 'lf'), '\n'),
  CRLF: copiarConFinDeLinea(path.join(temporal, 'crlf'), '\r\n'),
};
const resultados = Object.fromEntries(
  Object.entries(variantes).map(([nombre, carpeta]) => [nombre, verificar(carpeta)]),
);

const enlacesDe = (resultado, archivo) => resultado.enlaces.filter((e) => e.archivo === archivo);
const rotosDe = (resultado, archivo) => resultado.rotos.filter((r) => r.archivo === archivo);

describe('fines de línea', () => {
  test('la copia CRLF tiene CRLF y la LF no', () => {
    assert.match(fs.readFileSync(path.join(variantes.CRLF, 'titulos.md'), 'utf8'), /\r\n/);
    assert.doesNotMatch(fs.readFileSync(path.join(variantes.LF, 'titulos.md'), 'utf8'), /\r/);
  });

  test('CRLF y LF dan el mismo resultado', () => {
    assert.deepEqual(resultados.CRLF, resultados.LF);
  });
});

for (const [nombre, resultado] of Object.entries(resultados)) {
  describe(`ejemplos con ${nombre}`, () => {
    test('enlaces entre archivos, a carpetas, a la raíz y con anclas codificadas', () => {
      assert.equal(enlacesDe(resultado, 'basico.md').length, 5);
      assert.equal(enlacesDe(resultado, 'sub/destino.md').length, 2);
      assert.deepEqual(rotosDe(resultado, 'basico.md'), []);
      assert.deepEqual(rotosDe(resultado, 'sub/destino.md'), []);
    });

    test('títulos con código, tildes, puntuación, números con punto y espacios al final', () => {
      assert.equal(enlacesDe(resultado, 'titulos.md').length, 8);
      assert.deepEqual(rotosDe(resultado, 'titulos.md'), []);
    });

    test('títulos repetidos con sufijos -1 y -2', () => {
      assert.equal(enlacesDe(resultado, 'repetidos.md').length, 4);
      assert.deepEqual(rotosDe(resultado, 'repetidos.md'), [
        { archivo: 'repetidos.md', linea: 9, destino: '#notas-3', motivo: 'ancla inexistente' },
      ]);
    });

    test('no revisa enlaces en bloques de código ni en código en línea', () => {
      assert.deepEqual(enlacesDe(resultado, 'codigo.md'), []);
    });

    test('no revisa enlaces externos', () => {
      assert.deepEqual(enlacesDe(resultado, 'externos.md'), []);
    });

    test('marca el archivo inexistente, el ancla inexistente y el título dentro de un bloque', () => {
      assert.deepEqual(rotosDe(resultado, 'rotos.md'), [
        { archivo: 'rotos.md', linea: 3, destino: 'no-existe.md', motivo: 'archivo inexistente' },
        { archivo: 'rotos.md', linea: 5, destino: 'titulos.md#no-hay', motivo: 'ancla inexistente' },
        {
          archivo: 'rotos.md',
          linea: 7,
          destino: 'codigo.md#no-es-un-título',
          motivo: 'ancla inexistente',
        },
        {
          archivo: 'rotos.md',
          linea: 9,
          destino: 'Titulos.md',
          motivo: 'archivo inexistente: difieren las mayúsculas',
        },
        {
          archivo: 'rotos.md',
          linea: 11,
          destino: 'SUB/destino.md',
          motivo: 'archivo inexistente: difieren las mayúsculas',
        },
      ]);
    });

    test('no hay otros enlaces rotos', () => {
      assert.equal(resultado.rotos.length, 6);
    });
  });
}

describe('calcularSlug', () => {
  test('conserva el contenido del código y quita las comillas invertidas', () => {
    assert.equal(calcularSlug('1.2 Encabezado `X-Usuario-Id`'), '12-encabezado-x-usuario-id');
  });

  test('recorta los espacios al inicio y al final', () => {
    assert.equal(calcularSlug('  Título con espacios   '), 'título-con-espacios');
  });

  test('no colapsa los guiones que deja la puntuación', () => {
    assert.equal(calcularSlug('Enmienda 3 · 28/10/2026'), 'enmienda-3--28102026');
  });
});

describe('repositorio', () => {
  test('no cuenta los archivos de ejemplo de las pruebas', () => {
    const { archivos } = verificar(raizRepositorio);
    assert.ok(archivos.includes('README.md'));
    assert.deepEqual(archivos.filter((a) => a.startsWith('scripts/tests/fixtures/')), []);
  });
});

describe('línea de comandos', () => {
  const ejecutar = (carpeta) =>
    spawnSync(process.execPath, [script, carpeta], { encoding: 'utf8' });

  test('con enlaces rotos devuelve 1 y los lista con archivo y línea', () => {
    const salida = ejecutar(variantes.CRLF);
    assert.match(salida.stdout, /Enlaces revisados: 24/);
    assert.match(salida.stdout, /Enlaces rotos: 6/);
    assert.match(salida.stdout, /rotos\.md:3 {2}no-existe\.md {2}\(archivo inexistente\)/);
    assert.equal(salida.status, 1);
  });

  test('sin enlaces rotos devuelve 0', () => {
    const carpeta = copiarConFinDeLinea(path.join(temporal, 'sanos'), '\r\n', (ruta) =>
      ['basico.md', path.join('sub', 'destino.md'), 'titulos.md'].includes(ruta),
    );
    const salida = ejecutar(carpeta);
    assert.match(salida.stdout, /Enlaces revisados: 15/);
    assert.match(salida.stdout, /Enlaces rotos: 0/);
    assert.equal(salida.status, 0);
  });
});
