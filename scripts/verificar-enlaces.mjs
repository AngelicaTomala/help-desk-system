// Verifica los enlaces Markdown internos de los archivos .md del repositorio: que el archivo de
// destino exista y que el ancla exista, calculada con el algoritmo de GitHub
// (docs/auditoria-documentacion.md §13.1). Solo usa módulos integrados de Node.js.
//
// Uso:     node scripts/verificar-enlaces.mjs [carpeta]   (por defecto, la raíz del repositorio)
// Pruebas: node --test "scripts/tests/*.test.mjs"
//
// Código de salida: 0 si no hay enlaces rotos, 1 si hay alguno.

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const CARPETAS_IGNORADAS = new Set(['node_modules', 'bin', 'obj', '.git']);

// Los archivos de ejemplo de las pruebas tienen enlaces rotos a propósito.
export const RUTAS_EXCLUIDAS = [path.join('scripts', 'tests', 'fixtures')];

const ESQUEMA = /^[a-z][a-z0-9+.-]*:/i;
const APERTURA_BLOQUE = /^ {0,3}(`{3,}|~{3,})(.*)$/;
const TITULO = /^ {0,3}#{1,6}(?:[ \t]+(.*))?$/;
const ENLACE =
  /!?\[(?:[^[\]]|\[[^[\]]*\])*\]\(\s*(<[^>\n]*>|(?:[^\s()]|\([^\s()]*\))*)(?:\s+(?:"[^"]*"|'[^']*'|\([^)]*\)))?\s*\)/g;

export function listarMarkdown(raiz, excluidas = RUTAS_EXCLUIDAS) {
  const archivos = [];
  const recorrer = (carpeta) => {
    const entradas = fs
      .readdirSync(carpeta, { withFileTypes: true })
      .sort((a, b) => a.name.localeCompare(b.name));
    for (const entrada of entradas) {
      const ruta = path.join(carpeta, entrada.name);
      if (entrada.isDirectory()) {
        if (CARPETAS_IGNORADAS.has(entrada.name)) continue;
        if (excluidas.includes(path.relative(raiz, ruta))) continue;
        recorrer(ruta);
      } else if (entrada.isFile() && entrada.name.toLowerCase().endsWith('.md')) {
        archivos.push(ruta);
      }
    }
  };
  recorrer(raiz);
  return archivos;
}

// CRLF y CR se convierten en LF antes de cualquier análisis.
export const normalizarFinesDeLinea = (texto) => texto.replace(/\r\n?/g, '\n');

// Devuelve las líneas con el front matter y los bloques de código cercados en blanco, de modo que
// los números de línea se conservan.
export function quitarBloques(texto) {
  const lineas = normalizarFinesDeLinea(texto).split('\n');
  let i = 0;
  if (lineas[0] === '---') {
    const fin = lineas.findIndex((l, n) => n > 0 && (l === '---' || l === '...'));
    if (fin > 0) {
      for (; i <= fin; i++) lineas[i] = '';
    }
  }
  let cierre = null;
  for (; i < lineas.length; i++) {
    if (cierre) {
      const fin = lineas[i].match(/^ {0,3}(`{3,}|~{3,})[ \t]*$/);
      if (fin && fin[1][0] === cierre[0] && fin[1].length >= cierre.length) cierre = null;
      lineas[i] = '';
      continue;
    }
    const apertura = lineas[i].match(APERTURA_BLOQUE);
    if (apertura && !(apertura[1][0] === '`' && apertura[2].includes('`'))) {
      cierre = apertura[1];
      lineas[i] = '';
    }
  }
  return lineas;
}

// Reemplaza el código en línea por espacios. Una racha de n comillas invertidas se cierra con la
// siguiente racha de exactamente n; el código en línea no cruza una línea en blanco.
export function quitarCodigoEnLinea(lineas) {
  const texto = lineas.join('\n');
  const rachas = [...texto.matchAll(/`+/g)];
  let resultado = '';
  let copiado = 0;
  for (let r = 0; r < rachas.length; r++) {
    const apertura = rachas[r];
    if (apertura.index < copiado) continue;
    const cierre = rachas.find((c, n) => n > r && c[0].length === apertura[0].length);
    if (!cierre) continue;
    const fin = cierre.index + cierre[0].length;
    const tramo = texto.slice(apertura.index, fin);
    if (/\n[ \t]*\n/.test(tramo)) continue;
    resultado += texto.slice(copiado, apertura.index) + tramo.replace(/[^\n]/g, ' ');
    copiado = fin;
  }
  return (resultado + texto.slice(copiado)).split('\n');
}

// Ancla de un título según github-slugger: sin etiquetas HTML ni sintaxis de enlace, recortado, en
// minúsculas, sin nada que no sea letra, marca, número, conector, espacio o guion, y con cada
// espacio convertido en guion. Las comillas invertidas se quitan, pero su contenido se conserva.
export const calcularSlug = (titulo) =>
  titulo
    .replace(/<[^>]*>/g, '')
    .replace(/!?\[([^\]]*)\]\([^)]*\)/g, '$1')
    .trim()
    .toLowerCase()
    .replace(/[^\p{L}\p{M}\p{N}\p{Pc} -]/gu, '')
    .replace(/ /g, '-');

export function extraerTitulos(lineas) {
  const titulos = [];
  for (const linea of lineas) {
    const coincidencia = linea.match(TITULO);
    if (!coincidencia) continue;
    const texto = (coincidencia[1] ?? '').replace(/(^|[ \t]+)#+[ \t]*$/, '').trim();
    titulos.push(texto);
  }
  return titulos;
}

// Títulos repetidos: el segundo recibe el sufijo -1, el tercero -2, etc.
export function calcularAnclas(texto) {
  const apariciones = new Map();
  const anclas = new Set();
  for (const titulo of extraerTitulos(quitarBloques(texto))) {
    const original = calcularSlug(titulo);
    let ancla = original;
    while (apariciones.has(ancla)) {
      apariciones.set(original, apariciones.get(original) + 1);
      ancla = `${original}-${apariciones.get(original)}`;
    }
    apariciones.set(ancla, 0);
    anclas.add(ancla);
  }
  return anclas;
}

// Enlaces en línea fuera del código; los que tienen esquema (http, https, mailto…) son externos.
export function extraerEnlaces(texto) {
  const enlaces = [];
  quitarCodigoEnLinea(quitarBloques(texto)).forEach((linea, indice) => {
    for (const coincidencia of linea.matchAll(ENLACE)) {
      let destino = coincidencia[1];
      if (destino.startsWith('<')) destino = destino.slice(1, -1);
      if (destino === '' || ESQUEMA.test(destino)) continue;
      enlaces.push({ linea: indice + 1, destino });
    }
  });
  return enlaces;
}

const decodificar = (valor) => {
  try {
    return decodeURIComponent(valor);
  } catch {
    return valor;
  }
};

const rutaLegible = (raiz, ruta) => path.relative(raiz, ruta).split(path.sep).join('/');

// Como en GitHub, cada parte de la ruta debe coincidir, con sus mayúsculas, con un nombre que existe
// en su carpeta; en Windows fs.existsSync no distingue mayúsculas. Fuera de la raíz solo se comprueba
// que exista.
function existeConMayusculas(raiz, objetivo, nombresPorCarpeta) {
  const relativa = path.relative(raiz, objetivo);
  if (relativa === '') return true;
  if (relativa.startsWith('..') || path.isAbsolute(relativa)) return fs.existsSync(objetivo);
  let carpeta = raiz;
  for (const parte of relativa.split(path.sep)) {
    if (!nombresPorCarpeta.has(carpeta)) {
      let nombres = [];
      try {
        nombres = fs.readdirSync(carpeta);
      } catch {
        // No es una carpeta: ninguna parte puede colgar de ella.
      }
      nombresPorCarpeta.set(carpeta, new Set(nombres));
    }
    if (!nombresPorCarpeta.get(carpeta).has(parte)) return false;
    carpeta = path.join(carpeta, parte);
  }
  return true;
}

export function verificar(raiz) {
  const archivos = listarMarkdown(raiz);
  const textos = new Map();
  const anclasPorArchivo = new Map();
  const nombresPorCarpeta = new Map();
  const leer = (ruta) => {
    if (!textos.has(ruta)) textos.set(ruta, fs.readFileSync(ruta, 'utf8'));
    return textos.get(ruta);
  };
  const anclasDe = (ruta) => {
    if (!anclasPorArchivo.has(ruta)) anclasPorArchivo.set(ruta, calcularAnclas(leer(ruta)));
    return anclasPorArchivo.get(ruta);
  };

  const enlaces = [];
  const rotos = [];
  for (const archivo of archivos) {
    for (const { linea, destino } of extraerEnlaces(leer(archivo))) {
      const enlace = { archivo: rutaLegible(raiz, archivo), linea, destino };
      enlaces.push(enlace);

      const almohadilla = destino.indexOf('#');
      const ruta = decodificar(almohadilla < 0 ? destino : destino.slice(0, almohadilla));
      const ancla = almohadilla < 0 ? null : decodificar(destino.slice(almohadilla + 1));
      const objetivo =
        ruta === ''
          ? archivo
          : path.resolve(ruta.startsWith('/') ? raiz : path.dirname(archivo), ruta.replace(/^\/+/, ''));

      if (!fs.existsSync(objetivo)) {
        rotos.push({ ...enlace, motivo: 'archivo inexistente' });
      } else if (!existeConMayusculas(raiz, objetivo, nombresPorCarpeta)) {
        rotos.push({ ...enlace, motivo: 'archivo inexistente: difieren las mayúsculas' });
      } else if (
        ancla &&
        fs.statSync(objetivo).isFile() &&
        objetivo.toLowerCase().endsWith('.md') &&
        !anclasDe(objetivo).has(ancla)
      ) {
        rotos.push({ ...enlace, motivo: 'ancla inexistente' });
      }
    }
  }
  return { archivos: archivos.map((a) => rutaLegible(raiz, a)), enlaces, rotos };
}

function main() {
  const raiz = path.resolve(process.argv[2] ?? path.join(path.dirname(fileURLToPath(import.meta.url)), '..'));
  const { enlaces, rotos } = verificar(raiz);
  console.log(`Enlaces revisados: ${enlaces.length}`);
  console.log(`Enlaces rotos: ${rotos.length}`);
  for (const roto of rotos) {
    console.log(`  ${roto.archivo}:${roto.linea}  ${roto.destino}  (${roto.motivo})`);
  }
  process.exitCode = rotos.length > 0 ? 1 : 0;
}

// Se ejecuta como comando cuando Node recibe este archivo; en Windows las rutas pueden diferir en
// mayúsculas (letra de unidad, carpetas) según cómo se abrió la terminal.
const normalizarRuta = (ruta) => (process.platform === 'win32' ? ruta.toLowerCase() : ruta);
if (
  process.argv[1] &&
  normalizarRuta(path.resolve(process.argv[1])) === normalizarRuta(fileURLToPath(import.meta.url))
) {
  main();
}
