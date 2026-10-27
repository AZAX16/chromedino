"""Render the editable Report.md to a paginated A4 PDF (ReportLab + Pillow)."""
from pathlib import Path
from xml.sax.saxutils import escape
import re,sys
from reportlab.platypus import SimpleDocTemplate,Paragraph,Spacer,Image,Table,TableStyle,PageBreak,KeepTogether
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet,ParagraphStyle
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.lib.enums import TA_LEFT
from PIL import Image as PILImage
root=Path(__file__).resolve().parent
output=Path(sys.argv[1]) if len(sys.argv)>1 else root/'Report.pdf'
fontroot=Path('/usr/share/fonts/truetype/dejavu')
for n,f in [('Report','DejaVuSans.ttf'),('ReportBold','DejaVuSans-Bold.ttf'),('ReportItalic','DejaVuSans.ttf')]:pdfmetrics.registerFont(TTFont(n,str(fontroot/f)))
pdfmetrics.registerFontFamily('Report',normal='Report',bold='ReportBold',italic='ReportItalic',boldItalic='ReportBold')
styles=getSampleStyleSheet()
styles.add(ParagraphStyle(name='BodyReport',fontName='Report',fontSize=9,leading=12,spaceAfter=7,splitLongWords=True))
styles.add(ParagraphStyle(name='HeadingReport',fontName='ReportBold',fontSize=18,leading=22,spaceAfter=12,textColor=colors.HexColor('#202d3f')))
styles.add(ParagraphStyle(name='SubheadingReport',fontName='ReportBold',fontSize=11,leading=14,spaceBefore=5,spaceAfter=6))
styles.add(ParagraphStyle(name='CaptionReport',fontName='ReportItalic',fontSize=8,leading=10,spaceAfter=8,textColor=colors.HexColor('#455267')))
styles.add(ParagraphStyle(name='CellReport',fontName='Report',fontSize=8.3,leading=10.8))
def inline(s):
 s=escape(s)
 s=re.sub(r'\[([^\]]+)\]\((https?://[^)]+)\)',r'<link href="\2" color="#174b80">\1</link>',s)
 s=re.sub(r'\*\*([^*]+)\*\*',r'<b>\1</b>',s)
 s=re.sub(r'`([^`]+)`',r'<font color="#303f57">\1</font>',s)
 return s
lines=(root/'Report.md').read_text().splitlines();story=[];i=0
while i<len(lines):
 line=lines[i].strip()
 if not line:i+=1;continue
 if line=='<!-- pagebreak -->':story.append(PageBreak());i+=1;continue
 if line.startswith('# '):story.append(Paragraph(inline(line[2:]),styles['HeadingReport']));i+=1;continue
 if line.startswith('## '):story.append(Paragraph(inline(line[3:]),styles['SubheadingReport']));i+=1;continue
 if line.startswith('|'):
  rows=[]
  while i<len(lines) and lines[i].strip().startswith('|'):
   cells=[c.strip() for c in lines[i].strip().strip('|').split('|')]
   if not all(re.fullmatch(r'[-: ]+',c) for c in cells):rows.append([Paragraph(inline(c),styles['CellReport']) for c in cells])
   i+=1
  table=Table(rows,colWidths=[126,384],repeatRows=1,hAlign='LEFT')
  table.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),colors.HexColor('#dce6f0')),('ROWBACKGROUNDS',(0,1),(-1,-1),[colors.HexColor('#f2f5f8'),colors.white]),('VALIGN',(0,0),(-1,-1),'TOP'),('LEFTPADDING',(0,0),(-1,-1),6),('RIGHTPADDING',(0,0),(-1,-1),6),('TOPPADDING',(0,0),(-1,-1),5),('BOTTOMPADDING',(0,0),(-1,-1),5),('LINEBELOW',(0,0),(-1,0),.5,colors.HexColor('#a4b5c9'))]))
  story.extend([table,Spacer(1,9)]);continue
 if line.startswith('!['):
  m=re.fullmatch(r'!\[([^\]]*)\]\(([^)]+)\)',line);p=root/m.group(2);w,h=PILImage.open(p).size
  scale=min(510/w,255/h);im=Image(str(p),width=w*scale,height=h*scale);im.hAlign='CENTER'
  story.append(KeepTogether([im,Spacer(1,4),Paragraph(inline(m.group(1)),styles['CaptionReport'])]));i+=1;continue
 para=[line];i+=1
 while i<len(lines) and lines[i].strip() and not lines[i].startswith(('#','|','![','<!--')):para.append(lines[i].strip());i+=1
 story.append(Paragraph(inline(' '.join(para)),styles['BodyReport']))
def footer(c,d):
 c.setStrokeColor(colors.HexColor('#c6cfdb'));c.line(42,35,552,35);c.setFont('Report',7);c.setFillColor(colors.HexColor('#596579'));c.drawString(42,23,'Alireza Pooyan | 400414039 | Homework 1');c.drawRightString(552,23,str(d.page))
output.parent.mkdir(parents=True,exist_ok=True)
doc=SimpleDocTemplate(str(output),pagesize=(595.276,841.89),rightMargin=42,leftMargin=42,topMargin=37,bottomMargin=46,title='Chrome Dino Runner — Homework 1',author='Alireza Pooyan')
doc.build(story,onFirstPage=footer,onLaterPages=footer)
print(output)
