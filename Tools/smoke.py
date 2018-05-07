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
    tmpimg = Image.new('RGBA', [w, h], (0x00,0x00,0x00,0x00))
    tmp_pixels = tmpimg.load()
    for ky in range(h):
        for kx in range(w):
            p = src_pixels[kx, ky];
            tmp_pixels[kx, ky] = (0xff,
                                  0xff,
                                  0xff,
                                  p[3])
        #end
    #end
    tmpimg.save("out2.png")

    dstimg = Image.new('RGBA', [512, 512], (0x00,0x00,0x00,0x00))
    s = 0
    d = 0
    src_pixels = tmpimg.load()
    dst_pixels = dstimg.load()
    for i in range(4):
        #region = srcimg.crop((0, s, 128, s+512))
        #dstimg.paste(region, (d, 0))
        crop(src_pixels, dst_pixels, 0, s, 128, 512, d, 0)
        s += 512
        d += 128
    #end
    return dstimg
#end

if __name__ == '__main__':
    args = sys.argv
    src = args[1]
    srcimg = Image.open(src, 'r')
    dstimg = convert(srcimg)
    dstimg.save("out.png")
#EOF
