/* 
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
using jp.nyatla.nyartoolkit.cs.core;

namespace jp.nyatla.nyartoolkit.cs.detector
{
    class NyARDetectMarkerResult
    {
        public int arcode_id;

        public int direction;

        public double confidence;

        public NyARSquare ref_square;
    }

    class NyARDetectMarkerResultHolder
    {
        public NyARDetectMarkerResult[] result_array = new NyARDetectMarkerResult[1];

        /**
         * result_holderを最大i_reserve_size個の要素を格納できるように予約します。
         * 
         * @param i_reserve_size
         */
        public void reservHolder(int i_reserve_size)
        {
            if (i_reserve_size >= result_array.Length)
            {
                int new_size = i_reserve_size + 5;
                result_array = new NyARDetectMarkerResult[new_size];
                for (int i = 0; i < new_size; i++)
                {
                    result_array[i] = new NyARDetectMarkerResult();
                }
            }
        }
    }

    /**
     * 複数のマーカーを検出し、それぞれに最も一致するARコードを、コンストラクタで登録したARコードから 探すクラスです。最大300個を認識しますが、ゴミラベルを認識したりするので100個程度が限界です。
     * 
     */
    public class NyARDetectMarker
    {
        private const int AR_SQUARE_MAX = 300;

        private bool _is_continue = false;

        private NyARMatchPatt_Color_WITHOUT_PCA[] _match_patt;

        private INyARSquareDetector _square_detect;

        private NyARSquareStack _square_list = new NyARSquareStack(AR_SQUARE_MAX);

        protected INyARTransMat _transmat;

        private double[] _marker_width;

        // 検出結果の保存用
        private INyARColorPatt _patt;

        private NyARDetectMarkerResultHolder _result_holder = new NyARDetectMarkerResultHolder();
        private NyARMatchPattDeviationColorData _deviation_data;
        /**
         * 複数のマーカーを検出し、最も一致するARCodeをi_codeから検索するオブジェクトを作ります。
         * 
         * @param i_param
         * カメラパラメータを指定します。
         * @param i_code
         * 検出するマーカーのARCode配列を指定します。
         * 配列要素のインデックス番号が、そのままgetARCodeIndex関数で得られるARCodeインデックスになります。 
         * 例えば、要素[1]のARCodeに一致したマーカーである場合は、getARCodeIndexは1を返します。
         * @param i_marker_width
         * i_codeのマーカーサイズをミリメートルで指定した配列を指定します。 先頭からi_number_of_code個の要素には、有効な値を指定する必要があります。
         * @param i_number_of_code
         * i_codeに含まれる、ARCodeの数を指定します。
         * @param i_input_raster_type
         * 入力ラスタのピクセルタイプを指定します。この値は、INyARBufferReaderインタフェイスのgetBufferTypeの戻り値を指定します。
         * @throws NyARException
         */
        public NyARDetectMarker(NyARParam i_param, NyARCode[] i_code, double[] i_marker_width, int i_number_of_code, int i_input_raster_type)
        {
            initInstance(i_param, i_code, i_marker_width, i_number_of_code, i_input_raster_type);
            return;
        }
        protected void initInstance(
            NyARParam i_ref_param,
            NyARCode[] i_ref_code,
            double[] i_marker_width,
            int i_number_of_code,
            int i_input_raster_type)
        {

            NyARIntSize scr_size = i_ref_param.getScreenSize();
            // 解析オブジェクトを作る

            this._transmat = new NyARTransMat(i_ref_param);
            //各コード用の比較器を作る。
            this._match_patt = new NyARMatchPatt_Color_WITHOUT_PCA[i_number_of_code];
            int cw = i_ref_code[0].getWidth();
            int ch = i_ref_code[0].getHeight();
            this._match_patt[0] = new NyARMatchPatt_Color_WITHOUT_PCA(i_ref_code[0]);
            for (int i = 1; i < i_number_of_code; i++)
            {
                //解像度チェック
                if (cw != i_ref_code[i].getWidth() || ch != i_ref_code[i].getHeight())
                {
                    throw new NyARException();
                }
                this._match_patt[i] = new NyARMatchPatt_Color_WITHOUT_PCA(i_ref_code[i]);
            }
            //NyARToolkitプロファイル
            this._patt = new NyARColorPatt_Perspective_O2(cw, ch, 4, 25);
            this._square_detect = new NyARSquareDetector_Rle(i_ref_param.getDistortionFactor(), i_ref_param.getScreenSize());
            this._tobin_filter = new NyARRasterFilter_ARToolkitThreshold(100, i_input_raster_type);

            //実サイズ保存
            this._marker_width = i_marker_width;
            //差分データインスタンスの作成
            this._deviation_data = new NyARMatchPattDeviationColorData(cw, ch);
            //２値画像バッファを作る
            this._bin_raster = new NyARBinRaster(scr_size.w, scr_size.h);
            return;
        }

        private NyARBinRaster _bin_raster;

        private NyARRasterFilter_ARToolkitThreshold _tobin_filter;
        private NyARMatchPattResult __detectMarkerLite_mr = new NyARMatchPattResult();

        /**
         * i_imageにマーカー検出処理を実行し、結果を記録します。
         * 
         * @param i_raster
         * マーカーを検出するイメージを指定します。
         * @param i_thresh
         * 検出閾値を指定します。0～255の範囲で指定してください。 通常は100～130くらいを指定します。
         * @return 見つかったマーカーの数を返します。 マーカーが見つからない場合は0を返します。
         * @throws NyARException
         */
        public int detectMarkerLite(INyARRgbRaster i_raster, int i_threshold)
        {
            // サイズチェック
            if (!this._bin_raster.getSize().isEqualSize(i_raster.getSize()))
            {
                throw new NyARException();
            }

            // ラスタを２値イメージに変換する.
            this._tobin_filter.setThreshold(i_threshold);
            this._tobin_filter.doFilter(i_raster, this._bin_raster);

            NyARSquareStack l_square_list = this._square_list;
            // スクエアコードを探す
            this._square_detect.detectMarker(this._bin_raster, l_square_list);

            int number_of_square = l_square_list.getLength();
            // コードは見つかった？
            if (number_of_square < 1)
            {
                // ないや。おしまい。
                return 0;
            }
            // 保持リストのサイズを調整
            this._result_holder.reservHolder(number_of_square);
            NyARMatchPattResult mr = this.__detectMarkerLite_mr;

            // 1スクエア毎に、一致するコードを決定していく
            for (int i = 0; i < number_of_square; i++)
            {
                NyARSquare square = (NyARSquare)l_square_list.getItem(i);

                // 評価基準になるパターンをイメージから切り出す
                if (!this._patt.pickFromRaster(i_raster, square))
                {
                    // イメージの切り出しは失敗することもある。
                    continue;
                }
                //取得パターンをカラー差分データに変換する。
                this._deviation_data.setRaster(this._patt);
                int square_index = 0;
                int direction = NyARSquare.DIRECTION_UNKNOWN;
                double confidence = 0;
                for (int i2 = 0; i2 < this._match_patt.Length; i2++)
                {
                    this._match_patt[i2].evaluate(this._deviation_data, mr);

                    double c2 = mr.confidence;
                    if (confidence > c2)
                    {
                        continue;
                    }
                    // もっと一致するマーカーがあったぽい
                    square_index = i2;
                    direction = mr.direction;
                    confidence = c2;
                }
                // i番目のパターン情報を記録する。
                NyARDetectMarkerResult result = this._result_holder.result_array[i];
                result.arcode_id = square_index;
                result.confidence = confidence;
                result.direction = direction;
                result.ref_square = square;
            }
            return number_of_square;
        }

        /**
         * i_indexのマーカーに対する変換行列を計算し、結果値をo_resultへ格納します。 直前に実行したdetectMarkerLiteが成功していないと使えません。
         * 
         * @param i_index
         * マーカーのインデックス番号を指定します。 直前に実行したdetectMarkerLiteの戻り値未満かつ0以上である必要があります。
         * @param o_result
         * 結果値を受け取るオブジェクトを指定してください。
         * @throws NyARException
         */
        public void getTransmationMatrix(int i_index, NyARTransMatResult o_result)
        {
            NyARDetectMarkerResult result = this._result_holder.result_array[i_index];
            // 一番一致したマーカーの位置とかその辺を計算
            if (_is_continue)
            {
                _transmat.transMatContinue(result.ref_square, result.direction, _marker_width[result.arcode_id], o_result);
            }
            else
            {
                _transmat.transMat(result.ref_square, result.direction, _marker_width[result.arcode_id], o_result);
            }
            return;
        }

        /**
         * i_indexのマーカーの一致度を返します。
         * 
         * @param i_index
         * マーカーのインデックス番号を指定します。 直前に実行したdetectMarkerLiteの戻り値未満かつ0以上である必要があります。
         * @return マーカーの一致度を返します。0～1までの値をとります。 一致度が低い場合には、誤認識の可能性が高くなります。
         * @throws NyARException
         */
        public double getConfidence(int i_index)
        {
            return this._result_holder.result_array[i_index].confidence;
        }

        /**
         * i_indexのマーカーの方位を返します。
         * 
         * @param i_index
         * マーカーのインデックス番号を指定します。 直前に実行したdetectMarkerLiteの戻り値未満かつ0以上である必要があります。
         * @return 0,1,2,3の何れかを返します。
         */
        public int getDirection(int i_index)
        {
            return this._result_holder.result_array[i_index].direction;
        }

        /**
         * i_indexのマーカーのARCodeインデックスを返します。
         * 
         * @param i_index
         * マーカーのインデックス番号を指定します。 直前に実行したdetectMarkerLiteの戻り値未満かつ0以上である必要があります。
         * @return
         */
        public int getARCodeIndex(int i_index)
        {
            return this._result_holder.result_array[i_index].arcode_id;
        }

        /**
         * getTransmationMatrixの計算モードを設定します。
         * 
         * @param i_is_continue
         * TRUEなら、transMatContinueを使用します。 FALSEなら、transMatを使用します。
         */
        public void setContinueMode(bool i_is_continue)
        {
            this._is_continue = i_is_continue;
        }
    }

}