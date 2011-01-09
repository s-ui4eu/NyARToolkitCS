﻿/* 
 * PROJECT: NyARToolkitCS
 * --------------------------------------------------------------------------------
 * This work is based on the original ARToolKit developed by
 *   Hirokazu Kato
 *   Mark Billinghurst
 *   HITLab, University of Washington, Seattle
 * http://www.hitl.washington.edu/artoolkit/
 *
 * The NyARToolkit is Java version ARToolkit class library.
 * Copyright (C)2008 R.Iizuka
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this framework; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * 
 * For further information please contact.
 *	http://nyatla.jp/nyatoolkit/
 *	<airmail(at)ebony.plala.or.jp>
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace jp.nyatla.nyartoolkit.cs.core
{
    public class NyARRgbPixelReader_INT1D_X8R8G8B8_32 : INyARRgbPixelReader
    {
        protected int[] _ref_buf;

        private NyARIntSize _size;

        public NyARRgbPixelReader_INT1D_X8R8G8B8_32(int[] i_buf, NyARIntSize i_size)
        {
            this._ref_buf = i_buf;
            this._size = i_size;
        }

        public void getPixel(int i_x, int i_y, int[] o_rgb)
        {
            int rgb = this._ref_buf[i_x + i_y * this._size.w];
            o_rgb[0] = (rgb >> 16) & 0xff;// R
            o_rgb[1] = (rgb >> 8) & 0xff;// G
            o_rgb[2] = rgb & 0xff;// B
            return;
        }

        public void getPixelSet(int[] i_x, int[] i_y, int i_num, int[] o_rgb)
        {
            int width = this._size.w;
            int[] ref_buf = this._ref_buf;
            for (int i = i_num - 1; i >= 0; i--)
            {
                int rgb = ref_buf[i_x[i] + i_y[i] * width];
                o_rgb[i * 3 + 0] = (rgb >> 16) & 0xff;// R
                o_rgb[i * 3 + 1] = (rgb >> 8) & 0xff;// G
                o_rgb[i * 3 + 2] = rgb & 0xff;// B
            }
            return;
        }
    }

}