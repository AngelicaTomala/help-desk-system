"""Genera presentacion/HelpDesk_Proyecto_Integrador.pptx a partir del guion de presentacion/contenido.md.

Uso (desde la raíz del repositorio):
    python presentacion/generar.py

Requiere python-pptx (pip install python-pptx). Todo el contenido sale del repositorio; ver contenido.md.
"""

import re
from pathlib import Path

from lxml import etree
from pptx import Presentation
from pptx.dml.color import RGBColor
from pptx.enum.shapes import MSO_CONNECTOR, MSO_SHAPE
from pptx.enum.text import MSO_ANCHOR, PP_ALIGN
from pptx.oxml.ns import qn
from pptx.util import Emu, Inches, Pt

SALIDA = Path(__file__).resolve().parent / "HelpDesk_Proyecto_Integrador.pptx"

# Paleta: azul oscuro + acento ámbar.
AZUL = RGBColor(0x0B, 0x1F, 0x3A)
AZUL_MEDIO = RGBColor(0x1E, 0x3A, 0x66)
AZUL_CLARO = RGBColor(0xE8, 0xEE, 0xF7)
ACENTO = RGBColor(0xF2, 0xA9, 0x00)
TEXTO = RGBColor(0x1F, 0x29, 0x37)
GRIS = RGBColor(0x6B, 0x72, 0x80)
GRIS_CLARO = RGBColor(0xF3, 0xF4, 0xF6)
BLANCO = RGBColor(0xFF, 0xFF, 0xFF)
VERDE = RGBColor(0x16, 0xA3, 0x4A)
VERDE_CODIGO = RGBColor(0x4A, 0xDE, 0x80)
ROJO_CODIGO = RGBColor(0xF8, 0x71, 0x71)
FONDO_CODIGO = RGBColor(0x11, 0x18, 0x27)
TEXTO_CODIGO = RGBColor(0xE5, 0xE7, 0xEB)

FUENTE = "Segoe UI"
MONO = "Consolas"

ANCHO = Inches(13.333)
ALTO = Inches(7.5)
MARGEN = Inches(0.6)
TOTAL = 19

prs = Presentation()
prs.slide_width = ANCHO
prs.slide_height = ALTO
VACIA = prs.slide_layouts[6]


# ---------- utilidades ----------

def sin_corte(cadena):
    """Usa guiones de no separación en IDs (E-017, RN-03, S-02) y en '--' para que no se partan al final de línea."""
    cadena = re.sub(r"\b([A-Z]{1,3})-(\d)", "\\1\u2011\\2", cadena)
    return cadena.replace("--", "\u2011\u2011")


def fuente(run, tam, color=TEXTO, negrita=False, mono=False, cursiva=False):
    run.font.name = MONO if mono else FUENTE
    run.font.size = Pt(tam)
    run.font.bold = negrita
    run.font.italic = cursiva
    run.font.color.rgb = color


def caja(slide, x, y, w, h, relleno=None, borde=None, forma=MSO_SHAPE.RECTANGLE, grosor=1):
    s = slide.shapes.add_shape(forma, int(x), int(y), int(w), int(h))
    if relleno is None:
        s.fill.background()
    else:
        s.fill.solid()
        s.fill.fore_color.rgb = relleno
    if borde is None:
        s.line.fill.background()
    else:
        s.line.color.rgb = borde
        s.line.width = Pt(grosor)
    s.shadow.inherit = False
    return s


def texto(slide, x, y, w, h, lineas, tam=16, color=TEXTO, negrita=False, alinear=PP_ALIGN.LEFT,
          ancla=MSO_ANCHOR.TOP, mono=False, espacio=4, margen=Inches(0.05)):
    """lineas: str o lista de str / (str, dict) con claves tam, color, negrita, mono, cursiva."""
    tb = slide.shapes.add_textbox(int(x), int(y), int(w), int(h))
    tf = tb.text_frame
    tf.word_wrap = True
    tf.vertical_anchor = ancla
    tf.margin_left = tf.margin_right = margen
    tf.margin_top = tf.margin_bottom = Inches(0.03)
    if isinstance(lineas, str):
        lineas = [lineas]
    for i, linea in enumerate(lineas):
        opciones = {}
        if isinstance(linea, tuple):
            linea, opciones = linea
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.alignment = alinear
        p.space_after = Pt(espacio)
        r = p.add_run()
        es_mono = opciones.get("mono", mono)
        r.text = linea if es_mono else sin_corte(linea)
        fuente(r, opciones.get("tam", tam), opciones.get("color", color), opciones.get("negrita", negrita),
               es_mono, opciones.get("cursiva", False))
    return tb


def vinetas(slide, x, y, w, h, items, tam=17, color=TEXTO, espacio=8):
    """items: str o (texto_en_negrita, resto)."""
    tb = slide.shapes.add_textbox(x, y, w, h)
    tf = tb.text_frame
    tf.word_wrap = True
    for i, item in enumerate(items):
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.space_after = Pt(espacio)
        marcador = p.add_run()
        marcador.text = "▪  "
        fuente(marcador, tam, ACENTO, True)
        if isinstance(item, tuple):
            r = p.add_run()
            r.text = sin_corte(item[0])
            fuente(r, tam, color, True)
            r = p.add_run()
            r.text = sin_corte(item[1])
            fuente(r, tam, color)
        else:
            r = p.add_run()
            r.text = sin_corte(item)
            fuente(r, tam, color)
    return tb


def notas(slide, contenido):
    slide.notes_slide.notes_text_frame.text = contenido.strip()


def base(numero, titulo, subtitulo=None):
    slide = prs.slides.add_slide(VACIA)
    caja(slide, 0, 0, ANCHO, ALTO, BLANCO)
    caja(slide, 0, 0, ANCHO, Inches(1.05), AZUL)
    caja(slide, 0, Inches(1.05), ANCHO, Inches(0.06), ACENTO)
    texto(slide, MARGEN, Inches(0.16), Inches(11.4), Inches(0.75), titulo, 28, BLANCO, True,
          ancla=MSO_ANCHOR.MIDDLE)
    if subtitulo:
        texto(slide, MARGEN, Inches(1.2), Inches(12.1), Inches(0.45), subtitulo, 14, GRIS)
    # Pie
    texto(slide, MARGEN, Inches(7.05), Inches(8), Inches(0.35),
          "Help Desk System · Angélica Tomalá Torres · 31/10/2026", 10, GRIS)
    texto(slide, Inches(11.2), Inches(7.05), Inches(1.53), Inches(0.35), f"{numero} / {TOTAL - 1}", 10, GRIS,
          alinear=PP_ALIGN.RIGHT)
    return slide


def tabla(slide, x, y, anchos, filas, tam=12, alto_fila=Inches(0.4), encabezado=True, mono_cols=(),
          tam_encabezado=None):
    n_filas, n_cols = len(filas), len(anchos)
    forma = slide.shapes.add_table(n_filas, n_cols, x, y, sum(anchos, Emu(0)), alto_fila * n_filas)
    t = forma.table
    # Quita el estilo con bandas por defecto.
    tbl_pr = forma._element.graphic.graphicData.tbl.tblPr
    tbl_pr.set("bandRow", "0")
    tbl_pr.set("firstRow", "0")
    for i, w in enumerate(anchos):
        t.columns[i].width = w
    for r in range(n_filas):
        t.rows[r].height = alto_fila
        for c in range(n_cols):
            celda = t.cell(r, c)
            celda.margin_left = celda.margin_right = Inches(0.06)
            celda.margin_top = celda.margin_bottom = Inches(0.03)
            celda.vertical_anchor = MSO_ANCHOR.MIDDLE
            valor = filas[r][c]
            es_encabezado = encabezado and r == 0
            celda.fill.solid()
            if es_encabezado:
                celda.fill.fore_color.rgb = AZUL
            else:
                celda.fill.fore_color.rgb = AZUL_CLARO if r % 2 == 0 else BLANCO
            tf = celda.text_frame
            tf.word_wrap = True
            opciones = {}
            if isinstance(valor, tuple):
                valor, opciones = valor
            tf.paragraphs[0].text = ""
            r0 = tf.paragraphs[0].add_run()
            es_mono = (c in mono_cols and not es_encabezado) or opciones.get("mono", False)
            r0.text = valor if es_mono else sin_corte(valor)
            fuente(r0, (tam_encabezado or tam) if es_encabezado else opciones.get("tam", tam),
                   BLANCO if es_encabezado else opciones.get("color", TEXTO),
                   es_encabezado or opciones.get("negrita", False), es_mono)
    return t


def codigo(slide, x, y, w, h, lineas, tam=11, titulo=None):
    """Bloque de código: las líneas que empiezan por '+' van en verde y las de '-', en rojo."""
    caja(slide, x, y, w, h, FONDO_CODIGO, forma=MSO_SHAPE.ROUNDED_RECTANGLE).adjustments[0] = 0.03
    tb = slide.shapes.add_textbox(x + Inches(0.15), y + Inches(0.1), w - Inches(0.3), h - Inches(0.2))
    tf = tb.text_frame
    tf.word_wrap = True
    primera = True
    if titulo:
        p = tf.paragraphs[0]
        r = p.add_run()
        r.text = titulo
        fuente(r, tam - 1, ACENTO, True, mono=True)
        p.space_after = Pt(4)
        primera = False
    for linea in lineas:
        p = tf.paragraphs[0] if primera else tf.add_paragraph()
        primera = False
        p.space_after = Pt(1)
        color = TEXTO_CODIGO
        if linea.startswith("+"):
            color = VERDE_CODIGO
        elif linea.startswith("-"):
            color = ROJO_CODIGO
        r = p.add_run()
        r.text = linea
        fuente(r, tam, color, mono=True)
    return tb


def flecha(slide, x1, y1, x2, y2, color=AZUL_MEDIO, grosor=2, punteada=False):
    # Las coordenadas en EMU deben ser enteras o PowerPoint no abre el archivo.
    c = slide.shapes.add_connector(MSO_CONNECTOR.STRAIGHT, int(x1), int(y1), int(x2), int(y2))
    c.line.color.rgb = color
    c.line.width = Pt(grosor)
    ln = c.line._get_or_add_ln()
    if punteada:
        prst = etree.SubElement(ln, qn("a:prstDash"))
        prst.set("val", "dash")
    cola = etree.SubElement(ln, qn("a:tailEnd"))
    cola.set("type", "triangle")
    cola.set("w", "med")
    cola.set("len", "med")
    return c


def tarjeta(slide, x, y, w, h, titulo, cuerpo, tam_titulo=17, tam_cuerpo=14, color_borde=ACENTO):
    caja(slide, x, y, w, h, GRIS_CLARO)
    caja(slide, x, y, Inches(0.08), h, color_borde)
    texto(slide, x + Inches(0.2), y + Inches(0.12), w - Inches(0.3), Inches(0.5), titulo, tam_titulo, AZUL, True)
    if isinstance(cuerpo, list):
        vinetas(slide, x + Inches(0.15), y + Inches(0.6), w - Inches(0.25), h - Inches(0.7), cuerpo, tam_cuerpo,
                espacio=4)
    else:
        texto(slide, x + Inches(0.2), y + Inches(0.6), w - Inches(0.3), h - Inches(0.7), cuerpo, tam_cuerpo)


# ---------- 0 · Portada ----------

s = prs.slides.add_slide(VACIA)
caja(s, 0, 0, ANCHO, ALTO, AZUL)
caja(s, 0, Inches(4.45), ANCHO, Inches(0.08), ACENTO)
texto(s, Inches(0.9), Inches(1.5), Inches(11.5), Inches(1.2), "Help Desk System", 54, BLANCO, True)
texto(s, Inches(0.9), Inches(2.7), Inches(11.5), Inches(1.4),
      ["API REST de tickets de soporte de TI con control de SLA",
       (".NET 10 · Clean Architecture · desarrollo asistido por IA", {"tam": 20, "color": ACENTO})],
      24, BLANCO)
texto(s, Inches(0.9), Inches(4.85), Inches(11.5), Inches(1.8),
      [("Angélica Tomalá Torres", {"tam": 22, "negrita": True}),
       "Proyecto integrador · Agentic Software Development .NET 10 · Galaxy Training",
       "31/10/2026"], 18, BLANCO, espacio=6)
notas(s, """
Buenos días. Soy Angélica Tomalá Torres y voy a presentar mi proyecto integrador del curso Agentic Software Development con .NET 10: Help Desk System, una API REST para gestionar tickets de soporte de TI con control de SLA. Durante los próximos minutos les voy a contar qué problema resuelve, cómo lo especifiqué antes de escribir código, cómo está construida la solución y, sobre todo, cómo usé Claude Code como asistente sin perder el control de las decisiones. Toda la evidencia que voy a mostrar está en el repositorio: commits, bitácora de interacciones con la IA, especificaciones y pruebas. Al final haré una demostración en vivo desde Swagger UI y les mostraré el resultado real de la compilación y de las pruebas.
""")

# ---------- 1 · Problema ----------

s = base(1, "El problema: soporte técnico sin sistema")
texto(s, MARGEN, Inches(1.4), Inches(12.1), Inches(0.6),
      "En el área de TI las solicitudes llegan por correo, llamadas o mensajes sueltos.", 20, AZUL, True)
problemas = [
    ("Solicitudes perdidas", "Se pierden o se atienden dos veces."),
    ("Sin prioridad", "Un problema crítico espera igual que uno menor."),
    ("Sin trazabilidad", "No se sabe quién atiende qué ni en qué estado está cada solicitud."),
    ("Sin métricas", "Ninguna medición del cumplimiento de los tiempos de respuesta (SLA)."),
]
ancho_t, alto_t = Inches(5.9), Inches(1.8)
for i, (t, c) in enumerate(problemas):
    x = MARGEN + (ancho_t + Inches(0.3)) * (i % 2)
    y = Inches(2.3) + (alto_t + Inches(0.3)) * (i // 2)
    tarjeta(s, x, y, ancho_t, alto_t, t, c, 22, 18)
texto(s, MARGEN, Inches(6.65), Inches(12), Inches(0.4), "Fuente: specs/functional/00-vision.md §1", 11, GRIS)
notas(s, """
El punto de partida es una situación muy común en las áreas de TI. Las solicitudes llegan por cualquier canal: un correo, una llamada, un mensaje. Eso provoca cuatro problemas concretos que documenté en la visión del producto. Primero, solicitudes que se pierden o que dos técnicos atienden a la vez. Segundo, no hay prioridad: un sistema de nómina caído espera lo mismo que un monitor sin imagen. Tercero, no hay trazabilidad: nadie puede decir quién está atendiendo qué ni en qué estado está. Y cuarto, no hay métricas: no se puede saber si se cumplen los tiempos de respuesta comprometidos. El objetivo del proyecto es centralizar ese ciclo de vida en una API: registro, asignación a un técnico, seguimiento por estados y control del SLA según la prioridad.
""")

# ---------- 2 · Valor ----------

s = base(2, "El valor del producto: para quién y qué resuelve")
roles = [
    ("Solicitante", ["Crea tickets", "Consulta los suyos", "Cierra o reabre uno resuelto"]),
    ("Técnico", ["Ve sus tickets asignados", "Inicia la atención y resuelve", "Reclasifica sus tickets"]),
    ("Supervisor", ["Ve todos los tickets", "Asigna, reasigna y cambia la prioridad", "Reporte de vencidos"]),
]
ancho_r = Inches(3.9)
for i, (rol, items) in enumerate(roles):
    tarjeta(s, MARGEN + (ancho_r + Inches(0.2)) * i, Inches(1.35), ancho_r, Inches(2.1), rol, items, 19, 15)

texto(s, MARGEN, Inches(3.6), Inches(12), Inches(0.4), "Ciclo de vida controlado (RN-08): cualquier otra transición responde 409",
      15, AZUL, True)
estados = ["Abierto", "Asignado", "EnProgreso", "Resuelto", "Cerrado"]
acciones = ["Asignar", "Iniciar atención", "Resolver", "Cerrar"]
ancho_e, alto_e, sep = Inches(1.75), Inches(0.7), Inches(0.62)
y_e = Inches(4.5)
for i, estado in enumerate(estados):
    x = MARGEN + Inches(0.3) + (ancho_e + sep) * i
    forma = caja(s, x, y_e, ancho_e, alto_e, AZUL if estado != "Cerrado" else AZUL_MEDIO,
                 forma=MSO_SHAPE.ROUNDED_RECTANGLE)
    tf = forma.text_frame
    tf.paragraphs[0].alignment = PP_ALIGN.CENTER
    r = tf.paragraphs[0].add_run()
    r.text = estado
    fuente(r, 15, BLANCO, True)
    if i < 4:
        x1 = x + ancho_e
        flecha(s, x1, y_e + alto_e / 2, x1 + sep, y_e + alto_e / 2, ACENTO, 2.25)
        texto(s, x1 - Inches(0.25), y_e - Inches(0.42), sep + Inches(0.5), Inches(0.4), acciones[i], 10, GRIS,
              alinear=PP_ALIGN.CENTER)
texto(s, MARGEN + Inches(0.3), Inches(5.3), Inches(12), Inches(0.4),
      "Reasignar: Asignado / EnProgreso → Asignado (Supervisor)  ·  Reabrir: Resuelto → EnProgreso (Solicitante dueño)",
      13, GRIS)
caja(s, MARGEN, Inches(5.85), Inches(12.1), Inches(0.95), AZUL_CLARO)
texto(s, MARGEN + Inches(0.2), Inches(5.9), Inches(11.8), Inches(0.85),
      [("SLA automático por prioridad (RN-10):  Crítica 2 h · Alta 4 h · Media 8 h · Baja 24 h", {"negrita": True}),
       "Horas calendario 24/7 en UTC; el estado del SLA se calcula en cada consulta, no se guarda (RN-11)."],
      15, AZUL, ancla=MSO_ANCHOR.MIDDLE, espacio=2)
notas(s, """
El sistema tiene tres roles. El solicitante es el colaborador que reporta un problema; el técnico es quien lo atiende; y el supervisor coordina el área. Cada operación tiene una matriz de permisos explícita: por ejemplo, solo el solicitante crea tickets y solo el técnico asignado puede resolverlos. El valor concreto está en dos reglas. La primera es la máquina de estados: un ticket nace Abierto, el supervisor lo asigna, el técnico inicia la atención y lo resuelve, y el solicitante lo cierra o lo reabre. Cualquier otra transición responde 409. La segunda es el SLA: el vencimiento se calcula automáticamente desde la prioridad, en horas calendario y en UTC, y el estado del SLA se calcula en cada consulta, no se guarda. Así el supervisor puede ver qué tickets están vencidos sin depender de nadie.
""")

# ---------- 3 · Requisitos funcionales ----------

s = base(3, "Especificaciones: requisitos funcionales")
filas = [
    ["RF", "Nombre", "Historias", "CA", "Prioridad"],
    ["RF-01", "Registrar y reclasificar el ticket", "HU-01, HU-09", "10", "Obligatorio"],
    ["RF-02", "Consultar, filtrar y paginar tickets", "HU-02, HU-03, HU-13", "8", "Obligatorio"],
    ["RF-03", "Asignar y reasignar técnico", "HU-04, HU-11, HU-14", "8", "Obligatorio"],
    ["RF-04", "Cambiar estado del ticket", "HU-05, HU-06, HU-12", "9", "Obligatorio"],
    ["RF-05", "Calcular y controlar el SLA", "HU-07, HU-10", "7", "Obligatorio"],
    ["RF-06", "Reporte de tickets vencidos", "HU-08", "3", "Deseable"],
]
tabla(s, MARGEN, Inches(1.4), [Inches(1.2), Inches(5.0), Inches(3.0), Inches(1.0), Inches(1.9)], filas, 15,
      Inches(0.5))
cifras = [("6", "RF"), ("14", "HU"), ("45", "CA"), ("13", "RN"), ("10", "RNF"), ("13", "operaciones"),
          ("99", "decisiones")]
ancho_c = Inches(1.58)
for i, (n, etiqueta) in enumerate(cifras):
    x = MARGEN + (ancho_c + Inches(0.17)) * i
    caja(s, x, Inches(5.25), ancho_c, Inches(1.35), AZUL)
    texto(s, x, Inches(5.3), ancho_c, Inches(0.7), n, 30, ACENTO, True, PP_ALIGN.CENTER)
    texto(s, x, Inches(5.98), ancho_c, Inches(0.5), etiqueta, 13, BLANCO, alinear=PP_ALIGN.CENTER)
texto(s, MARGEN, Inches(6.65), Inches(12), Inches(0.4),
      "CA en formato Dado / Cuando / Entonces, con el código HTTP esperado · Fuente: 01-requisitos-funcionales.md",
      11, GRIS)
notas(s, """
Antes de escribir una sola línea de código, el proyecto pasó por una fase completa de especificación. Hay seis requisitos funcionales, cinco obligatorios y uno deseable, el reporte de vencidos. Cada RF se descompone en historias de usuario y cada historia en criterios de aceptación con formato Dado, Cuando, Entonces, con el código HTTP esperado. En total son 14 historias y 45 criterios. Además hay 13 reglas de negocio, 10 requisitos no funcionales, un modelo de dominio, diagramas C4, cuatro ADR y un contrato de API con 13 operaciones. Todo esto pasó por una auditoría de documentación que empezó con veredicto "no lista", con 55 hallazgos, y terminó en "lista" después de registrar decisiones. Hoy la auditoría tiene 99 decisiones numeradas, y el código las cita en comentarios y pruebas.
""")

# ---------- 4 · Matriz de trazabilidad ----------

s = base(4, "Matriz de trazabilidad · RF-01 (implementado)")
filas = [
    ["RF", "HU", "CU", "CA", "Endpoint", "Clase que lo implementa", "Tests que lo validan", "Commit"],
    ["RF-01", "HU-01", "CU-01", "CA-01.1", "POST /tickets", "CrearTicketHandler\nTicket.Crear",
     "CrearTicketEndpointTests (2)\nCrearTicketHandlerTests (1)", "530463f\neebf67e"],
    ["RF-01", "HU-01", "CU-01", "CA-01.3", "POST /tickets", "CrearTicketValidator",
     "CrearTicketValidatorTests (5)\nCrearTicketEndpointTests (4)", "530463f\neebf67e"],
    ["RF-01", "HU-01", "CU-01", "CA-01.5", "POST /tickets", "MiddlewareUsuarioActual",
     "CrearTicketEndpointTests (1)", "6851b9c\neebf67e"],
    ["RF-01", "HU-01", "CU-01", "CA-01.10", "POST /tickets", "CrearTicketHandler (403)",
     "CrearTicketEndpointTests (2)\nCrearTicketHandlerTests (1)", "530463f\neebf67e"],
    ["RF-01", "HU-09", "CU-02", "CA-01.6", "POST /tickets/{id}/\nreclasificar",
     "ReclasificarTicketHandler\nTicket.Reclasificar",
     "ReclasificarTicketEndpointTests (3)\nTicketRepositorioIntegracionTests (2)", "abc4a3d\neebf67e"],
    ["RF-01", "HU-09", "CU-02", "CA-01.7", "POST /tickets/{id}/\nreclasificar", "Ticket.Reclasificar (409)",
     "TicketReclasificarCambiarPrioridadTests (1)\nReclasificarTicketEndpointTests (1)", "7a04a34\nabc4a3d"],
]
tabla(s, Inches(0.4), Inches(1.35),
      [Inches(0.75), Inches(0.75), Inches(0.75), Inches(0.95), Inches(1.75), Inches(2.45), Inches(3.9),
       Inches(1.2)],
      filas, 11, Inches(0.68), mono_cols=(4, 5, 6, 7), tam_encabezado=12)
texto(s, Inches(0.4), Inches(6.3), Inches(12.5), Inches(0.7),
      ["Cada prueba lleva [Trait(\"CA\", …)] y [Trait(\"RN\", …)] (RNF-10):  dotnet test --filter \"CA=CA-01.3\"",
       "Fuentes: specs/trazabilidad.md · specs/casos-de-uso.md · git log"], 12, GRIS, espacio=2)
notas(s, """
La trazabilidad es un requisito no funcional del proyecto, el RNF-10. Cada prueba lleva un atributo Trait con el criterio de aceptación y la regla de negocio que valida, así que puedo ejecutar, por ejemplo, dotnet test con el filtro CA igual a CA-01.3 y ver solo las pruebas de ese criterio. En esta tabla muestro RF-01, que es el requisito implementado de punta a punta, y su cadena completa: requisito, historia, caso de uso, criterio, endpoint, clase, pruebas y commit. Los casos de uso CU-01, crear ticket, y CU-02, reclasificar ticket, están en specs/casos-de-uso.md y se derivan solo de las historias y criterios existentes. Por ejemplo, CA-01.3, que exige que el título tenga entre 5 y 150 caracteres, se implementa en el validador de FluentValidation y se verifica en dos niveles: cinco pruebas unitarias del validador y cuatro pruebas de extremo a extremo contra la API. Los 10 criterios de RF-01 tienen prueba, y la matriz completa con los nombres de cada test está en specs/trazabilidad.md.
""")

# ---------- 5 · Cobertura por RF ----------

s = base(5, "Cobertura de la trazabilidad por RF (estado real)")
cobertura = [
    ("RF-01", 10, 10, "2 / 2", "Implementado de punta a punta"),
    ("RF-02", 1, 8, "0 / 3", "Consulta del detalle en Infrastructure"),
    ("RF-03", 3, 8, "0 / 3", "Reglas listas en Domain"),
    ("RF-04", 5, 9, "0 / 4", "Reglas listas en Domain"),
    ("RF-05", 5, 7, "0 / 1", "SLA listo en Domain"),
    ("RF-06", 0, 3, "0 / 1", "Pendiente"),
]
y0 = Inches(1.45)
texto(s, MARGEN, y0, Inches(1.2), Inches(0.4), "RF", 13, GRIS, True)
texto(s, Inches(1.9), y0, Inches(5.0), Inches(0.4), "CA con prueba etiquetada", 13, GRIS, True)
texto(s, Inches(7.4), y0, Inches(1.6), Inches(0.4), "Endpoints", 13, GRIS, True)
texto(s, Inches(9.1), y0, Inches(3.6), Inches(0.4), "Estado", 13, GRIS, True)
barra = Inches(4.6)
for i, (rf, hechos, total, endpoints, estado) in enumerate(cobertura):
    y = Inches(1.95) + Inches(0.66) * i
    texto(s, MARGEN, y, Inches(1.2), Inches(0.45), rf, 17, AZUL, True, ancla=MSO_ANCHOR.MIDDLE)
    caja(s, Inches(1.9), y + Inches(0.08), barra, Inches(0.32), GRIS_CLARO)
    if hechos:
        caja(s, Inches(1.9), y + Inches(0.08), int(barra * hechos / total), Inches(0.32),
             VERDE if hechos == total else ACENTO)
    texto(s, Inches(6.55), y, Inches(0.85), Inches(0.45), f"{hechos}/{total}", 14, TEXTO, True,
          ancla=MSO_ANCHOR.MIDDLE)
    texto(s, Inches(7.4), y, Inches(1.6), Inches(0.45), endpoints, 15, TEXTO, ancla=MSO_ANCHOR.MIDDLE)
    texto(s, Inches(9.1), y, Inches(3.7), Inches(0.45), estado, 14, TEXTO, ancla=MSO_ANCHOR.MIDDLE)
caja(s, MARGEN, Inches(6.0), Inches(12.1), Inches(0.75), AZUL_CLARO)
texto(s, MARGEN + Inches(0.2), Inches(6.0), Inches(11.8), Inches(0.75),
      "24 de 45 CA y 11 de 13 RN con prueba etiquetada · prueba de arquitectura de RNF-07 obligatoria y en verde",
      15, AZUL, True, ancla=MSO_ANCHOR.MIDDLE)
notas(s, """
Quiero ser transparente con el estado real del proyecto. La capa de dominio está completa: los diez métodos del ticket, incluida la máquina de estados con sus 23 transiciones inválidas, el cálculo de SLA y la reasignación, ya tienen pruebas. Por eso RF-03, RF-04 y RF-05 ya tienen criterios cubiertos aunque todavía no tengan endpoint. Lo que está implementado de punta a punta, desde HTTP hasta SQL Server, es RF-01: crear y reclasificar tickets. Las otras 11 operaciones del contrato están especificadas, con sus errores y ejemplos, pero falta construir sus handlers y controladores. En total, 24 de los 45 criterios y 11 de las 13 reglas ya tienen una prueba etiquetada. Las dos reglas que faltan, RN-04 y RN-05, son de visibilidad y paginación, y llegarán con RF-02.
""")

# ---------- 6 · Arquitectura ----------

s = base(6, "Arquitectura: Clean Architecture")


def capa(slide, x, y, w, h, nombre, detalle, relleno, color_texto=BLANCO):
    forma = caja(slide, x, y, w, h, relleno, forma=MSO_SHAPE.ROUNDED_RECTANGLE)
    forma.adjustments[0] = 0.08
    texto(slide, x + Inches(0.1), y + Inches(0.08), w - Inches(0.2), Inches(0.45), nombre, 18, color_texto, True,
          PP_ALIGN.CENTER)
    texto(slide, x + Inches(0.1), y + Inches(0.52), w - Inches(0.2), h - Inches(0.6), detalle, 11, color_texto,
          alinear=PP_ALIGN.CENTER, mono=True, espacio=1)


cx = Inches(0.5)
capa(s, cx, Inches(1.45), Inches(3.25), Inches(1.45), "Api",
     ["TicketsController", "MiddlewareUsuarioActual", "ManejadorExcepciones", "OpenAPI"], AZUL_MEDIO)
capa(s, cx + Inches(4.3), Inches(1.45), Inches(3.25), Inches(1.45), "Infrastructure",
     ["HelpDeskDbContext", "migraciones · TicketRepositorio", "GeneradorCodigoTicket", "(SEQUENCE)"], AZUL_MEDIO)
capa(s, cx + Inches(1.28), Inches(3.45), Inches(5.0), Inches(1.35), "Application",
     ["Handlers · validadores FluentValidation", "ITicketRepositorio · IGeneradorCodigoTicket", "IUsuarioActual"],
     AZUL)
capa(s, cx + Inches(1.98), Inches(5.3), Inches(3.6), Inches(1.45), "Domain",
     ["Ticket · Usuario · Comentario", "enums · PoliticaSla", "excepciones de dominio"], ACENTO, AZUL)
# Dependencias hacia adentro.
flecha(s, cx + Inches(1.62), Inches(2.9), cx + Inches(2.6), Inches(3.45))
flecha(s, cx + Inches(5.92), Inches(2.9), cx + Inches(4.95), Inches(3.45))
flecha(s, cx + Inches(3.78), Inches(4.8), cx + Inches(3.78), Inches(5.3))
flecha(s, cx + Inches(3.25), Inches(1.95), cx + Inches(4.3), Inches(1.95), GRIS, 1.5, punteada=True)
texto(s, cx + Inches(3.25), Inches(2.05), Inches(1.05), Inches(0.55), ["solo", "Program.cs"], 10, GRIS,
      alinear=PP_ALIGN.CENTER, espacio=0, margen=0)
vinetas(s, Inches(8.2), Inches(1.45), Inches(4.7), Inches(5.3), [
    ("Dependencias hacia adentro: ", "Domain no referencia ningún proyecto ni paquete."),
    ("Api ", "solo traduce HTTP; usa Infrastructure únicamente para registrar dependencias."),
    ("Errores por capa: ", "Application 400 / 403 / 404 · Domain 409 · Api → Problem Details."),
    ("Verificado con 8 pruebas ", "de arquitectura (NetArchTest.Rules + los .csproj)."),
], 16, espacio=12)
notas(s, """
La solución sigue Clean Architecture en cuatro proyectos. En el centro está Domain, que contiene el ticket con su máquina de estados y el cálculo del SLA, y que no referencia ningún proyecto ni paquete. Application contiene los casos de uso: valida la entrada con FluentValidation, comprueba permisos y define las interfaces de persistencia. Infrastructure implementa esas interfaces con Entity Framework Core y SQL Server. Y la Api solo traduce HTTP: recibe la petición, llama al handler y convierte las excepciones a Problem Details. La regla de dependencias no queda solo en un diagrama: hay ocho pruebas de arquitectura que fallan si, por ejemplo, Domain llega a depender de Entity Framework o si un controlador usa Infrastructure directamente. La única excepción permitida es Program.cs, que registra las dependencias.
""")

# ---------- 7 · Stack ----------

s = base(7, "Stack tecnológico")
stack = [
    (".NET 10", "SDK 10.0.301 · ASP.NET Core Web API con controladores · solución HelpDesk.slnx"),
    ("Datos", "SQL Server LocalDB + EF Core 10.0.14 · SEQUENCE dbo.CodigoTicket para HD-000001"),
    ("Validación y errores", "FluentValidation 12.2.0 · Problem Details nativo de ASP.NET Core (RFC 9457)"),
    ("Documentación", "OpenAPI (Microsoft.AspNetCore.OpenApi 10.0.14) + Swagger UI (Swashbuckle 10.3.1)"),
    ("Pruebas", "xUnit · Mvc.Testing (WebApplicationFactory) · FakeTimeProvider · NetArchTest.Rules 1.3.2"),
    ("Sin Docker", "LocalDB y cadenas de conexión en user-secrets, nunca en el repositorio (RNF-02)"),
]
ancho_s, alto_s = Inches(5.9), Inches(1.6)
for i, (t, c) in enumerate(stack):
    x = MARGEN + (ancho_s + Inches(0.3)) * (i % 2)
    y = Inches(1.45) + (alto_s + Inches(0.2)) * (i // 2)
    tarjeta(s, x, y, ancho_s, alto_s, t, c, 19, 15)
notas(s, """
El stack es el que fija el curso. .NET 10 con ASP.NET Core Web API y controladores. La base de datos es SQL Server, en desarrollo LocalDB, con Entity Framework Core 10. El código del ticket, HD seguido de seis dígitos, sale de una SEQUENCE de SQL Server, lo que garantiza que no haya duplicados aunque se creen tickets a la vez. La validación usa FluentValidation y los errores usan Problem Details nativo, sin librerías externas. La documentación es OpenAPI con Swagger UI. Para las pruebas uso xUnit, WebApplicationFactory para las pruebas de extremo a extremo, FakeTimeProvider para controlar el reloj en las pruebas de SLA y NetArchTest para la regla de dependencias. No hay Docker: las cadenas de conexión se guardan en user-secrets, nunca en el repositorio. Un detalle: al crear los proyectos, la IA detectó dos paquetes transitivos vulnerables y los fijó a versiones corregidas.
""")

# ---------- 8 · ADR ----------

s = base(8, "Decisiones de arquitectura (ADR)")
filas = [
    ["ADR", "Decisión", "Alternativas descartadas", "Justificación"],
    ["001", "Clean Architecture en 4 proyectos", "N capas con entidades EF · un proyecto con carpetas · Vertical Slice",
     "La regla de dependencias se verifica en los .csproj (RNF-07); reglas en Domain (RNF-08)"],
    ["002", "SQL Server + EF Core", "Dapper · PostgreSQL / SQLite · EF Core InMemory",
     "SEQUENCE e índice único (RN-03); paginación en SQL (RNF-06); experiencia de la desarrolladora"],
    ["003", "FluentValidation en Application + invariantes en Domain",
     "DataAnnotations · todo en Domain · validar solo en Application",
     "400 con detalle por campo (RNF-05); un ticket inválido nunca puede existir (RNF-08)"],
    ["004", "Problem Details nativo + campo codigo", "Formato propio · try/catch por controlador · Result<T>",
     "Estándar RFC 9457 (RNF-03); códigos uniformes (RNF-04); 12 valores de codigo"],
]
tabla(s, MARGEN, Inches(1.4), [Inches(0.8), Inches(3.2), Inches(3.9), Inches(4.2)], filas, 13, Inches(0.95),
      tam_encabezado=14)
texto(s, MARGEN, Inches(6.35), Inches(12), Inches(0.5),
      "Formato Michael Nygard · los ADR aceptados no se reescriben: los cambios se agregan como enmiendas fechadas.",
      13, GRIS)
notas(s, """
Las cuatro decisiones de arquitectura están documentadas con el formato de Michael Nygard, y cada una descarta al menos dos alternativas con un motivo ligado a un requisito. Destaco dos. En el ADR-002 descarté EF Core InMemory para las pruebas, porque no aplica restricciones únicas ni SEQUENCE; por eso las pruebas de integración corren contra LocalDB real, y eso reveló comportamientos que un modelo en memoria nunca habría mostrado. En el ADR-003 la IA propuso validar solo en Application; yo lo corregí para que Domain también proteja las invariantes, porque un ticket con un título vacío no debe poder existir, ni siquiera en una prueba. Ese cambio lo muestro más adelante como diff. Una regla importante: un ADR aceptado no se reescribe; los cambios se agregan como enmiendas con fecha, para conservar el historial.
""")

# ---------- 9 · Flujo con Claude Code ----------

s = base(9, "Cómo usé Claude Code en el flujo de trabajo")
pasos = ["Especificar", "Auditar", "Planificar\n(modo Plan)", "Implementar\ncon pruebas", "Revisar y\nregistrar"]
ancho_p = Inches(2.45)
for i, paso in enumerate(pasos):
    forma = caja(s, MARGEN + (ancho_p - Inches(0.05)) * i, Inches(1.4), ancho_p, Inches(0.95),
                 AZUL if i % 2 == 0 else AZUL_MEDIO, forma=MSO_SHAPE.CHEVRON if i else MSO_SHAPE.PENTAGON)
    tf = forma.text_frame
    tf.paragraphs[0].alignment = PP_ALIGN.CENTER
    for j, parte in enumerate(paso.split("\n")):
        p = tf.paragraphs[0] if j == 0 else tf.add_paragraph()
        p.alignment = PP_ALIGN.CENTER
        r = p.add_run()
        r.text = parte
        fuente(r, 14, BLANCO, True)
vinetas(s, MARGEN, Inches(2.65), Inches(7.6), Inches(4.2), [
    ("Plantilla fija: ", "Contexto / Tarea / Restricciones / Formato / Verificación."),
    ("Memoria del proyecto: ", "CLAUDE.md con imports de las specs y reglas por capa en .claude/rules/."),
    ("MCP: ", "GitHub (PR y verificación de commits) y Playwright (Swagger UI)."),
    ("Modo Plan: ", "el plan se aprueba o se rechaza antes de escribir código."),
    ("Bitácora E-001 a E-017: ", "cada prompt, resultado y decisión, con su commit."),
], 16, espacio=10)
cifras = [("252", "elementos revisados", AZUL), ("198", "✅ aceptados", VERDE), ("38", "✏️ modificados", ACENTO),
          ("16", "❌ rechazados", RGBColor(0xDC, 0x26, 0x26))]
for i, (n, etiqueta, color) in enumerate(cifras):
    x = Inches(8.5) + Inches(2.2) * (i % 2)
    y = Inches(2.75) + Inches(1.85) * (i // 2)
    caja(s, x, y, Inches(2.0), Inches(1.65), GRIS_CLARO)
    caja(s, x, y, Inches(2.0), Inches(0.08), color)
    texto(s, x, y + Inches(0.2), Inches(2.0), Inches(0.75), n, 34, color, True, PP_ALIGN.CENTER)
    texto(s, x, y + Inches(1.0), Inches(2.0), Inches(0.5), etiqueta, 13, TEXTO, alinear=PP_ALIGN.CENTER)
notas(s, """
Esta es la sección más importante. Claude Code no escribió el proyecto solo: participó en cada fase con un flujo controlado. Todos los prompts siguen la misma plantilla: contexto, tarea, restricciones, formato y verificación. Las restricciones más útiles fueron "no modifiques otros archivos" y "si encuentras un hueco, no lo resuelvas: señálalo y pregúntame". El agente tiene memoria del proyecto en CLAUDE.md, que importa las reglas de negocio y el modelo de dominio, y reglas por capa que solo se cargan cuando trabaja en esa carpeta. Para el código usé el modo Plan: el agente propone, yo apruebo o rechazo, y solo entonces implementa. Cada interacción quedó en la bitácora con su prompt, el resultado y mi decisión. En las entradas E-001 a E-017 revisé 252 elementos: acepté 198, modifiqué 38 y rechacé 16. Eso es desarrollo asistido con revisión, no vibe coding.
""")

# ---------- 10 · Prompts clave ----------

s = base(10, "Prompts clave (resumidos, reales)")
prompts = [
    ("E-004 · Vincular el repositorio",
     ["«Si el remoto ya tiene commits, detente antes del push. No uses push --force. Si encuentras un secreto, detente.»",
      "El agente se detuvo y propuso merge / rebase / reset.",
      "Decisión: merge, sin reescribir el historial (a69d28d)."]),
    ("E-009 · Contrato en 6 partes",
     ["«No inventes endpoints, campos ni reglas; señala los huecos sin resolverlos; no hagas commit sin revisión.»",
      "13 operaciones con errores y ejemplos.",
      "33 decisiones nuevas (42 a 74) que se habrían tomado implícitamente."]),
    ("E-016 · Domain en modo Plan",
     ["«Firmas exactas del modelo, setters privados, sin DateTime.Now, cada prueba con [Trait(\"RN\")].»",
      "Plan rechazado con 4 correcciones: 28 → 23 transiciones inválidas.",
      "Resultado: 128 pruebas en verde."]),
]
ancho_k = Inches(3.95)
for i, (t, cuerpo) in enumerate(prompts):
    tarjeta(s, MARGEN + (ancho_k + Inches(0.12)) * i, Inches(1.45), ancho_k, Inches(4.05), t, cuerpo, 18, 15)
caja(s, MARGEN, Inches(5.75), Inches(12.1), Inches(0.95), AZUL)
texto(s, MARGEN + Inches(0.25), Inches(5.75), Inches(11.6), Inches(0.95),
      [("Restricción que más rindió (E-009):", {"color": ACENTO, "negrita": True}),
       "«Si encuentras un hueco, no lo resuelvas: señálalo y pregúntame.»"],
      16, BLANCO, ancla=MSO_ANCHOR.MIDDLE, espacio=2)
notas(s, """
Elegí tres prompts que muestran cómo dirigí al agente. El primero, al vincular el repositorio, le puse una condición de parada: si el remoto ya tenía commits, debía detenerse antes del push y no usar push force. El remoto tenía un commit creado desde la web, el agente se detuvo, me dio tres opciones y yo elegí merge para no reescribir el historial. El segundo es el contrato de API: un prompt así de grande no se podía controlar de una vez, así que lo dividí en seis partes con pausa de revisión. Gracias a la regla de "señala los huecos, no los resuelvas", salieron a la luz 33 decisiones que quedaron registradas. El tercero es la capa de dominio en modo Plan. Rechacé el primer plan porque contaba 28 transiciones inválidas cuando son 23: seis acciones por cinco estados, menos siete válidas. Revisar el plan antes del código evitó pruebas equivocadas.
""")

# ---------- 11 · Diffs ----------

s = base(11, "Evidencia de revisión: dos diffs reales")
codigo(s, MARGEN, Inches(1.35), Inches(8.3), Inches(2.6), [
    "- | `Crear(...)` | Valida invariantes, estado `Abierto`, sin técnico, …",
    "+ | `Crear(...)` | Valida las longitudes de RN-01 (excepción de dominio si se violan), …",
    "- | `Resolver(solucion, ahora)` | `EnProgreso` → `Resuelto`; fija FechaResolucion; …",
    "+ | `Resolver(solucion, ahora)` | Valida la solución (no vacía, ≤ TextoLongitudMaxima; …); …",
    "- | RN-09 | Domain               | `Ticket.Resolver` (solución obligatoria, …) …",
    "+ | RN-09 | Domain + Application | `Ticket.Resolver` / `Ticket.Reabrir` (invariante …) · …",
], 10, "✏️ CORREGIDO · eb0f1a1 · specs/technical/01-modelo-dominio.md")
tarjeta(s, Inches(9.1), Inches(1.35), Inches(3.65), Inches(2.6), "Criterio humano",
        "La IA propuso validar solo en Application y aplicó la corrección solo a RN-01. "
        "La extendí a RN-09: un ticket inválido nunca debe existir (RNF-08).", 15, 13)
codigo(s, MARGEN, Inches(4.2), Inches(8.3), Inches(2.6), [
    "  public void Resolver_TextoEnBlancoEnEstadoNoPermitido_LanzaInvarianteViolada()",
    "  {",
    "-     // El texto se valida antes que el estado (decisión de la desarrolladora en el plan de Domain).",
    "+     // El texto se valida antes que el estado (decisión 85).",
    "      AfirmarResolverRechazado(EstadoTicket.Abierto, \"\");",
], 10, "✅ ACEPTADO · 7577e08 · test/HelpDesk.Tests/Domain/TicketResolverReabrirTests.cs")
tarjeta(s, Inches(9.1), Inches(4.2), Inches(3.65), Inches(2.6), "Criterio humano",
        "La IA señaló que el comentario no era rastreable. Acepté: el código cita la decisión 85 registrada "
        "en la auditoría, no una conversación.", 15, 13, VERDE)
notas(s, """
Aquí están dos diffs reales del repositorio. El primero es una propuesta que corregí. Al redactar los ADR, la IA propuso que solo Application validara las longitudes del texto. Eso permitía crear un ticket con título vacío si alguien llamaba al dominio sin pasar por el validador. Le pedí que Domain también protegiera la regla, y cuando revisé el diff vi que lo había aplicado de forma literal, solo a RN-01. El mismo principio aplicaba a RN-09: un ticket resuelto sin solución también es un estado inválido. Lo extendí. La lección es que hay que comunicar el criterio, no solo el caso concreto. El segundo diff es una observación de la IA que acepté: dos comentarios de prueba decían "decisión de la desarrolladora en el plan", algo que nadie podría rastrear. Ahora citan la decisión 85, que está registrada en la auditoría.
""")

# ---------- 12 · Cuando la IA se equivocó ----------

s = base(12, "Más evidencia: cuando la IA se equivocó")
vinetas(s, MARGEN, Inches(1.4), Inches(6.9), Inches(5.4), [
    ("Cobertura falsa (E-016): ", "una prueba con Trait RN-06 documentaba lo contrario de la regla → se quitó."),
    ("Aviso incorrecto (E-015): ", "«settings.local.json se subiría» → ❌, ya estaba ignorado (git check-ignore)."),
    ("Arreglar el síntoma (E-017): ", "cambiar un enlace correcto para que el script no lo marcara → ❌, el error estaba en el script."),
    ("«La sintaxis es válida» (E-007): ", "el agente no renderizó los diagramas Mermaid."),
    ("Verificación independiente (E-010): ", "45/45 CA y 13/13 endpoints recontados con comandos propios."),
], 15, espacio=10)
codigo(s, Inches(7.75), Inches(1.6), Inches(5.0), Inches(2.55), [
    "[Fact]",
    "[Trait(\"RN\", \"RN-07\")]",
    "public void Reasignar_MismoTecnico_",
    "            NoLanzaExcepcion()",
    "{",
    "    // MISMO_TECNICO (RN-06) se valida en",
    "    // ReasignarTicketHandler (decisión 81);",
    "    // Domain no lo rechaza.",
], 11, "Código final · TicketTransicionesTests.cs")
texto(s, Inches(7.75), Inches(4.35), Inches(5.0), Inches(1.5),
      "El filtro dotnet test --filter \"RN=RN-06\" ya no cuenta esta prueba como cobertura de RN-06.", 13, GRIS)
notas(s, """
También quiero mostrar casos en que la IA se equivocó, porque ahí es donde la revisión humana marca la diferencia. El más interesante es una cobertura falsa: una prueba de dominio estaba etiquetada con RN-06, la regla que prohíbe reasignar al mismo técnico, pero la prueba demuestra justo lo contrario: que el dominio no lo rechaza, porque esa validación vive en el handler. Si la dejaba, la matriz de trazabilidad decía que RN-06 estaba probada cuando no lo estaba. Otro caso: el agente quiso cambiar un enlace correcto para que su propio script dejara de marcarlo como roto; lo rechacé, porque el error estaba en el script. Y aprendí que cuando el agente dice "verificado", tengo que repetir la verificación con mis propios comandos, mostrando los conteos.
""")

# ---------- 13 · Endpoints + captura ----------

s = base(13, "Demostración: endpoints documentados en OpenAPI")
filas = [
    ["Método", "Ruta", "Estado"],
    ["POST", "/api/v1/tickets", ("✅ Implementado", {"color": VERDE, "negrita": True})],
    ["POST", "/api/v1/tickets/{id}/reclasificar", ("✅ Implementado", {"color": VERDE, "negrita": True})],
    ["GET", "/api/v1/tickets · /tickets/{id}", "Especificado"],
    ["POST", "/tickets/{id}/asignar · reasignar", "Especificado"],
    ["POST", "/tickets/{id}/iniciar-atencion · resolver", "Especificado"],
    ["POST", "/tickets/{id}/cerrar · reabrir", "Especificado"],
    ["POST", "/tickets/{id}/cambiar-prioridad", "Especificado"],
    ["GET", "/tecnicos · /reportes/tickets-vencidos", "Especificado"],
]
tabla(s, Inches(0.4), Inches(1.35), [Inches(0.85), Inches(4.25), Inches(1.65)], filas, 12, Inches(0.5),
      mono_cols=(1,))
texto(s, Inches(0.4), Inches(5.95), Inches(6.75), Inches(0.9),
      ["Crear: 201 · 400 · 401 · 403 · 415 · 500", "Reclasificar: 200 · 400 · 401 · 403 · 404 · 409 · 415 · 500",
       "X-Usuario-Id y valores de enum documentados (RNF-09)"], 12, GRIS, espacio=1)
# Recuadro 16:9 para la captura de Swagger UI.
ancho_cap = Inches(5.4)
alto_cap = int(ancho_cap * 9 / 16)
x_cap, y_cap = Inches(7.35), Inches(1.75)
recuadro = caja(s, x_cap, y_cap, ancho_cap, alto_cap, GRIS_CLARO, GRIS, grosor=1.5)
recuadro.line.dash_style = 4
texto(s, x_cap, y_cap, ancho_cap, alto_cap,
      ["[Captura Swagger UI]", ("http://localhost:3001/swagger · 16:9", {"tam": 12, "color": GRIS})],
      20, GRIS, True, PP_ALIGN.CENTER, MSO_ANCHOR.MIDDLE)
texto(s, x_cap, y_cap + alto_cap + Inches(0.1), ancho_cap, Inches(0.4),
      "Documento OpenAPI: /openapi/v1.json (solo en Development)", 11, GRIS, alinear=PP_ALIGN.CENTER)
notas(s, """
Esta es la superficie de la API documentada en OpenAPI. Hoy hay dos operaciones implementadas: crear un ticket y reclasificarlo. Cada una documenta todas sus respuestas posibles, incluidos los errores, porque el requisito RNF-09 lo exige, y una prueba automática comprueba que el documento OpenAPI coincide con el contrato. El encabezado X-Usuario-Id aparece en cada operación, y los campos de categoría y prioridad muestran sus valores permitidos. Las otras once operaciones, como asignar, resolver o el reporte de vencidos, están especificadas en el contrato con sus rutas, cuerpos, errores y ejemplos, y la lógica de dominio que necesitan ya está probada. Lo que falta es construir sus handlers y controladores siguiendo el mismo patrón de RF-01.

--- GUION DE LA DEMO EN VIVO ---
Antes de presentar: dotnet user-secrets list --project src/HelpDesk.Api (cadena de conexión); dotnet ef migrations list --project src/HelpDesk.Infrastructure --startup-project src/HelpDesk.Api (sin "Pending"); cerrar cualquier instancia de la Api.
1. dotnet run --project src/HelpDesk.Api  → http://localhost:3001
2. Abrir http://localhost:3001/swagger y mostrar las dos operaciones; abrir http://localhost:3001/openapi/v1.json.
3. Crear (201): POST /api/v1/tickets, X-Usuario-Id: 1 (Ana), cuerpo válido de la diapositiva 14. Anotar el id; mostrar Location, codigo, fechaVencimiento (+4 h) y estadoSla.
4. Validación (400): mismo endpoint con el cuerpo inválido → VALIDACION_FALLIDA con 3 claves.
5. Identidad (401): sin X-Usuario-Id → USUARIO_NO_IDENTIFICADO; X-Usuario-Id: 999 → USUARIO_INEXISTENTE.
6. Permiso (403): crear con X-Usuario-Id: 6 (Fabián, técnico) → ROL_NO_AUTORIZADO.
7. Reclasificar (200): POST /api/v1/tickets/{id}/reclasificar, X-Usuario-Id: 10 (Jorge, supervisor), {"categoria": "Otros"}.
8. Permiso (403): con X-Usuario-Id: 1 → ROL_NO_AUTORIZADO; con X-Usuario-Id: 7 (técnico no asignado) → TICKET_AJENO.
9. Recurso (404): reclasificar el id 012 o uno inexistente → TICKET_NO_ENCONTRADO.
10. Cerrar con dotnet test en la terminal (o la diapositiva 15).
Nota: no hay GET implementado todavía ni se eliminan tickets; el 409 TICKET_NO_MODIFICABLE no se puede mostrar en vivo (no hay endpoint para resolver o cerrar), está cubierto por ReclasificarTicketEndpointTests.
""")

# ---------- 14 · Request válido e inválido ----------

s = base(14, "Reglas y validaciones: request válido e inválido")
codigo(s, MARGEN, Inches(1.35), Inches(5.95), Inches(4.55), [
    "POST /api/v1/tickets",
    "X-Usuario-Id: 1",
    "{ \"titulo\": \"No puedo instalar el antivirus\",",
    "  \"descripcion\": \"El instalador muestra un",
    "     error de permisos al iniciar.\",",
    "  \"categoria\": \"Software\",",
    "  \"prioridad\": \"Alta\" }",
    "",
    "+ 201 Created",
    "+ Location: /api/v1/tickets/{id}",
    "+ \"codigo\": \"HD-000001\", \"estado\": \"Abierto\",",
    "+ \"tecnico\": null, \"estadoSla\": \"EnTiempo\"",
    "  // vence 4 h después (Alta, RN-10)",
], 12, "VÁLIDO · contrato §2.1")
codigo(s, Inches(6.75), Inches(1.35), Inches(5.95), Inches(4.55), [
    "POST /api/v1/tickets",
    "X-Usuario-Id: 1",
    "{ \"titulo\": \"Red\",",
    "  \"descripcion\": \"     \",",
    "  \"categoria\": \"Impresoras\",",
    "  \"prioridad\": \"Alta\" }",
    "",
    "- 400 Bad Request · application/problem+json",
    "- \"codigo\": \"VALIDACION_FALLIDA\",",
    "- \"errors\": { \"titulo\": [...],",
    "-   \"descripcion\": [...], \"categoria\": [...] }",
    "  // el ticket no se crea (CA-01.3, CA-01.4)",
], 12, "INVÁLIDO · contrato §4.1")
caja(s, MARGEN, Inches(6.1), Inches(12.1), Inches(0.75), AZUL_CLARO)
texto(s, MARGEN + Inches(0.2), Inches(6.1), Inches(11.8), Inches(0.75),
      [("Orden de errores: 401 → 404 → 403 → 400 → 409", {"negrita": True}),
       "Un técnico con título inválido recibe 403, no 400 (Crear_TecnicoConTituloInvalido_Responde403YNoCrea)."],
      13, AZUL, ancla=MSO_ANCHOR.MIDDLE, espacio=1)
notas(s, """
Aquí vemos las reglas en acción con los ejemplos del contrato, que además son pruebas automáticas. A la izquierda, una petición válida de Ana, una solicitante: responde 201, con el encabezado Location, el código HD generado por la SEQUENCE, estado Abierto, sin técnico, y el vencimiento a las cuatro horas porque la prioridad es Alta. A la derecha, una petición con tres errores: un título de tres caracteres, una descripción que queda vacía al recortar los espacios y una categoría que no existe. La API responde 400 con Problem Details y los tres errores a la vez, uno por campo, y no crea el ticket. Algo que cuidé mucho es el orden de evaluación: si un técnico intenta crear un ticket con un título inválido, recibe 403 y no 400, porque primero se comprueba quién es y después qué envía.

Demo: pasos 3 a 6 del guion (notas de la diapositiva 13).
""")

# ---------- 15 · Build y pruebas ----------

s = base(15, "Resultado real: build y pruebas")
codigo(s, MARGEN, Inches(1.4), Inches(5.0), Inches(2.0), [
    "> dotnet build",
    "",
    "+ Compilación correcta.",
    "+     0 Advertencia(s)",
    "+     0 Errores",
], 15)
codigo(s, Inches(5.9), Inches(1.4), Inches(6.8), Inches(2.0), [
    "> dotnet test",
    "",
    "+ Correctas!  Con error: 0",
    "+ Superado: 472   Omitido: 0",
    "+ Total: 472      Duración: 39 s",
], 15)
filas = [
    ["Domain", "Application", "Infrastructure", "Integración (LocalDB)", "Api (extremo a extremo)", "Arquitectura"],
    ["48", "35", "28", "22", "72", "8"],
]
t = tabla(s, MARGEN, Inches(3.85), [Inches(2.01)] * 6, filas, 22, Inches(0.6), tam_encabezado=13)
for c in range(6):
    t.cell(1, c).text_frame.paragraphs[0].alignment = PP_ALIGN.CENTER
    t.cell(0, c).text_frame.paragraphs[0].alignment = PP_ALIGN.CENTER
texto(s, MARGEN, Inches(5.15), Inches(12), Inches(0.4), "Métodos [Fact] / [Theory] por carpeta de pruebas", 12, GRIS)
caja(s, MARGEN, Inches(5.75), Inches(12.1), Inches(0.95), AZUL_CLARO)
texto(s, MARGEN + Inches(0.2), Inches(5.75), Inches(11.8), Inches(0.95),
      [("213 métodos de prueba → 472 casos ejecutados", {"negrita": True}),
       "Ejecutado el 31/10/2026 sobre e6030c9 · incluye la SEQUENCE agotada en HD-999999 → 500 sin crear el ticket"],
      14, AZUL, ancla=MSO_ANCHOR.MIDDLE, espacio=2)
notas(s, """
Este es el resultado real, ejecutado hoy sobre el último commit de código de la rama. La solución compila sin errores y sin advertencias, algo que exigí desde el primer commit. Y las 472 pruebas pasan. Son 213 métodos de prueba que, con los casos parametrizados, se convierten en 472 ejecuciones. Están repartidos por capa: las de dominio prueban la máquina de estados y el SLA sin base de datos; las de Application usan dobles de prueba; las de integración corren contra una base LocalDB separada, que se borra y se recrea en cada ejecución; las de la Api levantan la aplicación completa con WebApplicationFactory y un reloj falso; y las de arquitectura vigilan la regla de dependencias. Incluso hay una prueba que agota la SEQUENCE hasta HD-999999 y comprueba que la API responde 500 sin crear el ticket.

Demo: pasos 7 a 10 del guion (notas de la diapositiva 13); terminar con dotnet test en la terminal.
""")

# ---------- 16 · Aprendizajes ----------

s = base(16, "Aprendizajes y retos superados")
vinetas(s, MARGEN, Inches(1.4), Inches(7.6), Inches(5.4), [
    ("Prompts por partes: ", "dividir el contrato con pausas hizo visibles 33 decisiones implícitas (E-009)."),
    ("Límites de la IA: ", "fuerte en coherencia documental; se le escapa el comportamiento por defecto de .NET (E-009)."),
    ("Verificar de nuevo: ", "repetir las verificaciones del agente con comandos propios y conteos (E-010, E-012)."),
    ("Plan antes que código: ", "revisar el plan y fijar un criterio de salida evita revisiones sin fin (E-013, E-016)."),
    ("Base real: ", "LocalDB reveló el RESTART de la SEQUENCE, invisible para un modelo en memoria (E-017)."),
], 16, espacio=12)
tarjeta(s, Inches(8.6), Inches(1.45), Inches(4.15), Inches(2.4), "Auditoría",
        "De NO LISTA a LISTA: 55 hallazgos resueltos antes de escribir el contrato (E-011).", 18, 14)
tarjeta(s, Inches(8.6), Inches(4.1), Inches(4.15), Inches(2.4), "Seguridad de paquetes",
        "Dos paquetes transitivos vulnerables detectados al crear la solución y fijados a versiones corregidas "
        "(E-014).", 18, 14, VERDE)
notas(s, """
Me llevo cinco aprendizajes. Primero, un prompt grande se controla mejor dividido en partes con pausas: en el contrato de API eso sacó a la luz 33 decisiones que la IA habría tomado sola. Segundo, la IA es excelente en coherencia documental, en cruces y conteos, pero se le escapan detalles del comportamiento por defecto de .NET, como que acepta enums numéricos o que EF Core lee las fechas sin la marca UTC; eso requiere conocimiento del framework. Tercero, cuando el agente dice "verificado", lo repito con mis propios comandos. Cuarto, revisar el plan antes del código y fijar cuándo detener una revisión evita ciclos infinitos. Y quinto, probar contra la base real: la SEQUENCE de SQL Server tenía un comportamiento al reiniciarse que ningún modelo en memoria habría mostrado.
""")

# ---------- 17 · Mejoras pendientes ----------

s = base(17, "Mejoras pendientes")
vinetas(s, MARGEN, Inches(1.4), Inches(7.3), Inches(5.4), [
    ("11 de 13 operaciones: ", "RF-02 listar y consultar · RF-03 asignar, reasignar y técnicos · RF-04 iniciar, resolver, cerrar y reabrir · RF-05 cambiar prioridad · RF-06 reporte."),
    ("Trazabilidad: ", "21 CA aún sin prueba y casos de uso de RF-02 a RF-06."),
    ("Proceso: ", "cerrar la entrada E-018 de la bitácora y fusionar feat/rf-01 en main mediante PR."),
    ("Límite conocido: ", "el código del ticket llega hasta HD-999999 (RN-03)."),
], 16, espacio=12)
tarjeta(s, Inches(8.3), Inches(1.45), Inches(4.45), Inches(3.9), "Fuera del MVP (visión §4)", [
    "Autenticación JWT (hoy X-Usuario-Id, S-02)",
    "Docker",
    "Notificaciones por correo",
    "SLA en horario laboral",
    "Historial de cambios",
    "Control de concurrencia",
], 18, 15)
notas(s, """
Para cerrar el proyecto quedan pendientes claros. El principal es implementar las once operaciones restantes. La buena noticia es que el camino ya está trazado: el dominio está probado, el contrato define cada respuesta y RF-01 dejó un patrón completo, con middleware, manejo de errores y pruebas de extremo a extremo, que se repite para cada operación. También quedan 21 criterios de aceptación sin prueba, que llegarán con esas operaciones, y los casos de uso del resto de requisitos. En el proceso, falta cerrar la entrada de la bitácora de RF-01 y fusionar la rama en main mediante un pull request. Y hay mejoras que quedaron fuera del MVP de forma consciente y documentada: autenticación con JWT en lugar del encabezado X-Usuario-Id, Docker, notificaciones por correo, SLA en horario laboral, historial de cambios y control de concurrencia.
""")

# ---------- 18 · Cierre ----------

s = prs.slides.add_slide(VACIA)
caja(s, 0, 0, ANCHO, ALTO, AZUL)
caja(s, 0, Inches(4.2), ANCHO, Inches(0.08), ACENTO)
texto(s, Inches(0.9), Inches(1.7), Inches(11.5), Inches(1.3), "Gracias", 60, BLANCO, True)
texto(s, Inches(0.9), Inches(2.95), Inches(11.5), Inches(0.9), "¿Preguntas?", 36, ACENTO, True)
texto(s, Inches(0.9), Inches(4.6), Inches(11.5), Inches(1.6),
      ["Desarrollo asistido con revisión, no vibe coding:",
       ("cada decisión de la IA tiene un prompt, un diff, un criterio humano y un commit.", {"negrita": True})],
      20, BLANCO, espacio=6)
texto(s, Inches(0.9), Inches(6.5), Inches(11.5), Inches(0.5), "Angélica Tomalá Torres · Help Desk System", 14,
      AZUL_CLARO)
notas(s, """
Para resumir: Help Desk System resuelve un problema real del soporte de TI con una API que controla el ciclo de vida del ticket y su SLA. Antes del código hubo especificaciones completas, una auditoría y un contrato. La arquitectura separa el negocio de la infraestructura, y eso se comprueba con pruebas. Y, sobre todo, la IA fue un asistente dirigido: cada propuesta pasó por mi revisión, quedó registrada en la bitácora con su decisión y llegó a un commit. Hoy son 472 pruebas en verde y una ruta clara para lo que falta. Muchas gracias por su atención. Quedo atenta a sus preguntas.
""")

assert len(prs.slides) == TOTAL, len(prs.slides)
prs.save(SALIDA)
print(f"Generado {SALIDA} con {len(prs.slides)} diapositivas.")
