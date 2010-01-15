﻿/* 
 * PROJECT: NyARToolkitCS
 * --------------------------------------------------------------------------------
 * This work is based on the original ARToolKit developed by
 *   Hirokazu Kato
 *   Mark Billinghurst
 *   HITLab, University of Washington, Seattle
 * http://www.hitl.washington.edu/artoolkit/
 *
 * The NyARToolkitCS is C# edition ARToolKit class library.
 * Copyright (C)2008-2009 Ryo Iizuka
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * For further information please contact.
 *	http://nyatla.jp/nyatoolkit/
 *	<airmail(at)ebony.plala.or.jp> or <nyatla(at)nyatla.jp>
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace jp.nyatla.nyartoolkit.cs.core
{
    /**このクラスは、単機能のNyARRasterです。
     * 特定タイプのバッファをラップする、INyARBufferReaderインタフェイスを提供します。
     *
     */
    public class NyARRaster : NyARRaster_BasicClass
    {
	    private NyARBufferReader _reader;
	    public override INyARBufferReader getBufferReader()
	    {
		    return this._reader;
	    }
        public NyARRaster(NyARIntSize i_size, object i_ref_buf, int i_buf_type)
            : base(i_size)
	    {
		    this._reader=new NyARBufferReader(i_ref_buf,i_buf_type);
		    return;
	    }
    }
}