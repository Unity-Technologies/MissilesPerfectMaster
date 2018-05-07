import sys
import os
import math
import random
import numpy as np
from PIL import Image 
from PIL import ImageDraw
from PIL import ImageFilter
from progressbar import ProgressBar

SIZE = 1024
#SIZE = 256
prgrs = 0

p = ProgressBar(0, SIZE*SIZE*2*6)

def add_img(src_pixels, w, h, dst_pixels, x, y, col):
    for ky in range(h):
        for kx in range(w):
            (sr, sg, sb) = src_pixels[kx, ky]
            (dr, dg, db) = dst_pixels[kx+x, ky+y]
            sr = int(float(sr) * col[0])
            sg = int(float(sg) * col[1])
            sb = int(float(sb) * col[2])
            dst_pixels[kx+x, ky+y] = (sr+dr, sg+dg, sb+db)
        #end
    #end
#end

def put_star(starimg, dstimg, num):
    dw, dh = dstimg.size
    w, h = starimg.size
    src_pixels = starimg.load();
    dst_pixels = dstimg.load()
    for n in range(num):
        x = random.randrange(0, dw-w-1)
        y = random.randrange(0, dh-h-1)
        val = random.randrange(2, 10)/10
        col = (val, val, val)
        if random.random() < 0.01:
            if random.random() < 0.5:
                col = (1.0,
                       0.5,
                       random.random()*0.5)
            else:
                col = (1.0,
                       random.random()*0.5+0.5,
                       0.5)
            #end
        #end
        add_img(src_pixels, w, h, dst_pixels, x, y, col)
    #end
#end

def put_point(field, power, num, mergin, w, h):
    num = random.randrange(0, num)
    for n in range(num):
        x = random.randrange(mergin, w-mergin)
        y = random.randrange(mergin, h-mergin)
        if random.randrange(10) > 0:
            field[y][x][0] += power
            field[y][x][1] += power
            field[y][x][2] += power
        elif random.randrange(3) < 1:
            field[y][x][0] += power
            field[y][x][1] += power/2
            field[y][x][2] += 0
        elif random.randrange(3) < 1:
            field[y][x][0] += power
            field[y][x][1] += 0
            field[y][x][2] += 0
        else:
            field[y][x][0] += 0
            field[y][x][1] += power
            field[y][x][2] += power
        #end
    #end
#end

def gaussian_blur(src_field, w, h, radius):
    global prgrs
    dst_field = np.zeros((h, w, 3), dtype='float')
    rs = math.ceil(radius * 2.57) 
    rad22 = 2*radius*radius
    pi2rad2 = math.pi*2*radius*radius
    for ky in range(h):
        for kx in range(w):
            val = np.zeros((3))
            wsum = np.zeros((3))
            prgrs = prgrs + 1
            p.update(prgrs)
            for repy in range(int(rs+rs+1)):
                iy = ky-rs+repy
                for repx in range(int(rs+rs+1)):
                    ix = kx-rs+repx
                    x = min(w-1, max(0, ix))
                    y = min(h-1, max(0, iy))
                    len2 = (ix-kx)*(ix-kx)+(iy-ky)*(iy-ky)
                    weight = math.exp(-len2/rad22)/pi2rad2
                    val[0] += src_field[int(y)][int(x)][0]*weight
                    val[1] += src_field[int(y)][int(x)][1]*weight
                    val[2] += src_field[int(y)][int(x)][2]*weight
                    wsum[0] += weight
                    wsum[1] += weight
                    wsum[2] += weight
                #end
            #end
            dst_field[ky][kx][0] = math.floor(val[0]/wsum[0])
            dst_field[ky][kx][1] = math.floor(val[1]/wsum[1])
            dst_field[ky][kx][2] = math.floor(val[2]/wsum[2])
        #end
    #end
    return dst_field
#end

def create_blured_star(size):
    w = size
    h = size
    field = np.zeros((h, w, 3), dtype='float')
    cx = (int)(w/2)
    cy = (int)(h/2)
    field[cy][cx][0] = 100.0 * w * w
    field[cy][cx][1] = 100.0 * w * w
    field[cy][cx][2] = 100.0 * w * w

    if w > 4:
        field = gaussian_blur(field, w, h, w/4)
    #end
    dstimg = Image.new('RGB', [w, h], (0x00,0x00,0x00))
    dst_pixels = dstimg.load()
    for ky in range(h):
        for kx in range(w):
            dst_pixels[kx, ky] = (max(0, min(255, int(field[ky][kx][0]))),
                                  max(0, min(255, int(field[ky][kx][1]))),
                                  max(0, min(255, int(field[ky][kx][2]))))
        #end
    #end
    return dstimg
#end

def create(w, h):
    global prgrs
    prgrs = 0
    dstimg = Image.new('RGB', [w, h], (0x00,0x00,0x00))
    dst_pixels = dstimg.load()
    starimg0 = create_blured_star(3)
    put_star(starimg0, dstimg, 3000)
    starimg1 = create_blured_star(4)
    put_star(starimg1, dstimg, 400)
    starimg2 = create_blured_star(5)
    put_star(starimg2, dstimg, 100)
    starimg3 = create_blured_star(6)
    put_star(starimg3, dstimg, 20)
    return dstimg
#end

if __name__ == '__main__':
    for i in range(6):
        dstimg = create(SIZE, SIZE)
        dstimg.save("out%d.png" % i)
    #end
#end
