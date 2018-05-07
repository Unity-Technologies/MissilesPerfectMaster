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
    dstimg = Image.new('RGBA', [w, h], (0x00,0x00,0x00,0x00))
    dst_pixels = dstimg.load()
    for ky in range(h):
        for kx in range(w/4):
            p0 = src_pixels[kx+w/4*0, ky];
            p1 = src_pixels[kx+w/4*1, ky];
            p2 = src_pixels[kx+w/4*2, ky];
            p3 = src_pixels[kx+w/4*3, ky];
            dst_pixels[kx, ky] = (p0[0],
                                  p1[0],
                                  p2[0],
                                  p3[0])
        #end
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
