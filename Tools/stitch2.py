import sys
import os
import math
from PIL import Image 
from PIL import ImageDraw
from PIL import ImageFilter

if __name__ == '__main__':
    args = sys.argv
    faces = ( "x+", "x-", "y+", "y-", "z+", "z-" )
    out_width = 1024
    start_x = 0
    img = Image.new('RGBA', [out_width*6, out_width], (0x00,0x00,0x00,0xff))
    for face in faces:
        srcimg = Image.open(face+".png", 'r')
        resized_img = srcimg.resize((out_width, out_width))
        clipboard = resized_img.crop((0, 0, out_width, out_width))
        img.paste(clipboard, (start_x, 0, start_x + out_width, out_width))
        start_x += out_width
    #end

    outdir = "."
    outpath = outdir + '/skybox.png'
    img.save(outpath);
#EOF
