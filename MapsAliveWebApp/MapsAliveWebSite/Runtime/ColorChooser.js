var columns;
var colorArray;
var colorFieldId;
var colorSwatchId;
var chooseColorForPreview;

function maAddColorToArray(color)
{
	colorArray[colorArray.length] = '#' + color;
}

function maOnClickColor(color)
{
	document.getElementById(colorSwatchId).style.backgroundColor = color;
	document.getElementById(colorFieldId).value = color;
	document.getElementById('colorPalette').style.display = 'none';
	maShowColorPalette(false);
	if (chooseColorForPreview)
		maChangeDetectedForPreview();
	else
		maChangeDetected();
	maColorChanged(colorSwatchId, color);
}

function maOnEditColor(swatchId, textBox, forPreview)
{
	var color = textBox.value;
	var s = document.getElementById(swatchId).style;
	if (forPreview)
		maChangeDetectedForPreview();
	else
		maChangeDetected();
	try
	{
		s.backgroundColor = color;
	}
	catch(e)
	{
		s.backgroundColor = "#ffffff";
	}
	maColorChanged(swatchId, color);
}

function maColorChanged(swatchId, color)
{
    if (typeof maOnColorChanged != "undefined")
        maOnColorChanged(swatchId, color);
}

function maCreateColorArray()
{
	columns = 17;
	colorArray = Array();

	// Black, Gray, White, Tan, Brown
	maAddColorToArray('000000');
	maAddColorToArray('333333');
	maAddColorToArray('666666');
	maAddColorToArray('888888');
	maAddColorToArray('999999');
	maAddColorToArray('aaaaaa');
	maAddColorToArray('cccccc');
	maAddColorToArray('eeeeee');

	maAddColorToArray('ffffff');

	maAddColorToArray('f9efe3');
	maAddColorToArray('f6f0d3');
	maAddColorToArray('dac9a4');
	maAddColorToArray('bd854a');
	maAddColorToArray('c17d11');
	maAddColorToArray('9c5f0c');
	maAddColorToArray('7c4f28');
	maAddColorToArray('572500');


	// Reds
	maAddColorToArray('cc0000');
	maAddColorToArray('ef2929');
	maAddColorToArray('ff0000');
	maAddColorToArray('ff2222');
	maAddColorToArray('ff4444');
	maAddColorToArray('ff6666');
	maAddColorToArray('ff8888');
	maAddColorToArray('ffaaaa');

	maAddColorToArray('ffdddd');

	maAddColorToArray('f2dcdb');
	maAddColorToArray('e5b9b7');
	maAddColorToArray('d99694');
	maAddColorToArray('af5a58');
	maAddColorToArray('a40000');
	maAddColorToArray('aa0000');
	maAddColorToArray('872929');
	maAddColorToArray('770000');
	
	// Pinks and Purples
	maAddColorToArray('aa00aa');
	maAddColorToArray('ff22ff');
	maAddColorToArray('ff66ff');
	maAddColorToArray('ffaaff');
	maAddColorToArray('f195be');
	maAddColorToArray('f9b6a6');
	maAddColorToArray('fcd2c2');
	maAddColorToArray('f5c5dd');

	maAddColorToArray('ffddff');

	maAddColorToArray('e5e0ec');
	maAddColorToArray('ccc1d9');
	maAddColorToArray('b9b3d9');
	maAddColorToArray('ad7fa8');
	maAddColorToArray('9a5aa4');
	maAddColorToArray('8064A2');
	maAddColorToArray('663399');
	maAddColorToArray('771d7d');
	
	// Oranges
	maAddColorToArray('d14009');
	maAddColorToArray('FF5500');
	maAddColorToArray('FF7700');
	maAddColorToArray('FF9900');
	maAddColorToArray('ffcc00');
	maAddColorToArray('ffff00');
	maAddColorToArray('ffff66');
	maAddColorToArray('ffffaa');
	
	maAddColorToArray('ffffdd');

	maAddColorToArray('ffeea5');	
	maAddColorToArray('ffe292');
	maAddColorToArray('fce94f');	
	maAddColorToArray('fdd24f');
	maAddColorToArray('e6b439');
	maAddColorToArray('e09919');
	maAddColorToArray('f2891e');
	maAddColorToArray('cd6619');
	
	// Greens
	maAddColorToArray('005500');
	maAddColorToArray('007700');
	maAddColorToArray('00aa00');
	maAddColorToArray('00ff00');
	maAddColorToArray('44ff44');
	maAddColorToArray('66ff66');
	maAddColorToArray('88ff88');
	maAddColorToArray('aaffaa');

	maAddColorToArray('ddffdd');

	maAddColorToArray('ebf1dd');
	maAddColorToArray('d7e3bc');
	maAddColorToArray('d3e27d');
	maAddColorToArray('9bbb59');
	maAddColorToArray('669044');
	maAddColorToArray('477f40');
	maAddColorToArray('4f612b');
	maAddColorToArray('356639');
	
	// Turquoise Blues
	maAddColorToArray('005599');
	maAddColorToArray('007799');
	maAddColorToArray('009999');
	maAddColorToArray('00cccc');
	maAddColorToArray('00ffff');
	maAddColorToArray('66ffff');
	maAddColorToArray('88ffff');
	maAddColorToArray('aaffff');
	
	maAddColorToArray('ddffff');

	maAddColorToArray('b7dde8');
	maAddColorToArray('92cddc');
	maAddColorToArray('6fbfd1');
	maAddColorToArray('00a2b1');
	maAddColorToArray('31859b');
	maAddColorToArray('007d6b');
	maAddColorToArray('006a5e');
	maAddColorToArray('205867');
	
	// Blues
	maAddColorToArray('000077');
	maAddColorToArray('0000cc');
	maAddColorToArray('0000ff');
	maAddColorToArray('0055ff');
	maAddColorToArray('4d91ff');
	maAddColorToArray('76abff');
	maAddColorToArray('b3d0ff');
	maAddColorToArray('d6e5fc');
	
	maAddColorToArray('dbe5f1');

	maAddColorToArray('b8cce4');
	maAddColorToArray('95b3d7');
	maAddColorToArray('729fcf');
	maAddColorToArray('548dd4');
	maAddColorToArray('3465a4');
	maAddColorToArray('1f497d');
	maAddColorToArray('1b3f95');
	maAddColorToArray('082366');

}

function maCreateColorPalette(id, forPreview)
{
	var palette = document.getElementById('colorPalette');
	if (palette === null)
	{
		maCreateColorArray();
		palette = document.createElement('div');
		palette.id = 'colorPalette';
		palette.style.position = 'absolute';
		palette.style.display = 'none';
		palette.style.border = '#000000 1px solid';
		palette.style.background = '#FFFFFF';
		palette.style.zIndex = 1;
		palette.innerHTML = '<div style="font-family:Verdana; text-align:left; font-size:11px;"><div align="right" style="background-color: #eeeeee;border-bottom: 1px solid #dddddd"><img onclick="maShowColorPalette(false)" src="../images/close.gif" width="10" height="10" border="0" style="padding: 1px;_margin: 1px"></div>' +
			maEmitColorPaletteTable() + '</div>';
		document.body.appendChild(palette);
	}
	chooseColorForPreview = forPreview;
	return palette;
}

function maShowColorPalette(show)
{
	var colorPalette = document.getElementById('colorPalette');
	if (colorPalette)
		colorPalette.style.display = show ? 'block' : 'none';
}

function maColorPaletteIsShowing(palette)
{
	return palette.style.display == 'block';
} 
     
function maChooseColor(swatchId, fieldId, forPreview)
{
	var palette = maCreateColorPalette(swatchId, forPreview);
	if (swatchId == colorSwatchId && maColorPaletteIsShowing(palette))
	{
		// The user clicked the swatch instead of a color in the palette.
		maShowColorPalette(false);	
		return;
	}
	colorFieldId = fieldId;
	colorSwatchId = swatchId;
	var pt = maPt(document.getElementById(swatchId));
	palette.style.top = (pt.y + 20)+"px";
	palette.style.left = (pt.x - 262) +"px";
	maShowColorPalette(true);
}

function maEmitColorPaletteTable()
{
	 var table = '<table border="0" cellspacing="1" cellpadding="1">';
	 for (i = 0; i < colorArray.length; i++)
	 {
		var color = colorArray[i];
		if (i % columns === 0)
			table += '<tr>';
		table += 
			'<td bgcolor="#000000"><span style="outline:1px solid #000000;' + 
			'cursor:pointer;background:' + color + ';font-size:10px;" ' +
			'onclick="maOnClickColor(\'' + color + '\');">&nbsp;&nbsp;&nbsp;</span></td>';
		if (i % columns == columns - 1)
			table += '</tr>';
	 }
	 if (i % columns !== 0)
		table += '</tr>';
	 table += '</table>';
	 return table;
}
