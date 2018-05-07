import sys
import os
import math
import random
from PIL import Image 
from PIL import ImageDraw
from PIL import ImageFilter

def crop(src_pixels, dst_pixels, x, y, w, h, dx, dy):
    for ky in range(h):
        for kx in range(w):
            p = src_pixels[x+kx, y+ky];
            dst_pixels[dx+kx, dy+ky] = p
        #end
    #end
#end

def convert(srcimg):
    w, h = srcimg.size
    #print("%d,%d" % (w, h))
    src_pixels = srcimg.load()
    dstimg = Image.new('RGBA', [int(w/4), int(h*4)], (0x00,0x00,0x00,0x00))

    clipboard = srcimg.crop((int(w/4*0), 0, int(w/4*1), h))
    dstimg.paste(clipboard, (0, 0, int(w/4), int(h*1)))
    clipboard = srcimg.crop((int(w/4*1), 0, int(w/4*2), h))
    dstimg.paste(clipboard, (0, h*1, int(w/4), int(h*2)))
    clipboard = srcimg.crop((int(w/4*2), 0, int(w/4*3), h))
    dstimg.paste(clipboard, (0, h*2, int(w/4), int(h*3)))
    clipboard = srcimg.crop((int(w/4*3), 0, int(w/4*4), h))
    dstimg.paste(clipboard, (0, h*3, int(w/4), int(h*4)))

    return dstimg
#end

if __name__ == '__main__':
    args = sys.argv
    src = args[1]
    srcimg = Image.open(src, 'r')
    dstimg = convert(srcimg)
    dstimg.save("out.png")
#EOF
