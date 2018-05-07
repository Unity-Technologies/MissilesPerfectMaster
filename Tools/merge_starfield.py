import sys
import os
import math
import random
import numpy as np
from PIL import Image 
from PIL import ImageDraw
from PIL import ImageFilter

def create(srcimg0, srcimg1):
    w0, h0 = srcimg0.size
    w1, h1 = srcimg1.size
    if w0 != w1:
        return
    #end
    if h0 != h1:
        return
    #end
    w, h = w0, h0

    dstimg = Image.new('RGB', [w, h], (0x00,0x00,0x00))
    dst_pixels = dstimg.load()
    src0_pixels = srcimg0.load()
    src1_pixels = srcimg1.load()
    th = 48
    for ky in range(h):
        for kx in range(w):
            p0 = src0_pixels[kx, ky]
            p1 = src1_pixels[kx, ky]
            ratio = min(th, max(p1[0], p1[1], p1[2]))/th
            r = (int)(p0[0]*(1-ratio) + p1[0]*ratio)
            g = (int)(p0[1]*(1-ratio) + p1[1]*ratio)
            b = (int)(p0[2]*(1-ratio) + p1[2]*ratio)
            dst_pixels[kx, ky] = (r, g, b)
        #end
    #end

    return dstimg
#end

def merge(src0, src1, dst):
    srcimg0 = Image.open(src0, 'r')
    srcimg1 = Image.open(src1, 'r')
    dstimg = create(srcimg0, srcimg1)
    dstimg.save(dst)
#end


if __name__ == '__main__':
    merge("out0.png", "x+.png", "x0+.png")
    merge("out1.png", "x-.png", "x0-.png")
    merge("out2.png", "y+.png", "y0+.png")
    merge("out3.png", "y-.png", "y0-.png")
#end


