using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTest1
{
	class ImageTest
	{
		public void testc(){
			String[] args = { };
			int i, sch = 0;
			float[] range_0 = { 0, 256 };
			float[][] ranges = { range_0 };
			double tmp, dist = 0;
			IplImage src_img1, src_img2;
			IplImage[] dst_img1 = new IplImage[4];
			IplImage[] dst_img2 = new IplImage[4];

			CvHistogram[] hist1 = new CvHistogram[4];
			CvHistogram hist2;

			// (1)二枚の画像を読み込む．チャンネル数が等しくない場合は，終了
			if (args.Count() < 2)
			{
				Console.WriteLine("Usage : TestHistgram <file> <folder>");
				return;
			}

			//templateフォルダ以下にテンプレート画像を入れておく。
			IEnumerable<string> tempFiles = Directory.EnumerateFiles(args[1], "*.jpg", SearchOption.TopDirectoryOnly);
			Dictionary<string, double> NCCPair = new Dictionary<string, double>();

			//(1)二枚の画像を読み込む．チャンネル数が等しくない場合は，終了
			src_img1 = IplImage.FromFile(args[0], LoadMode.AnyDepth | LoadMode.AnyColor);

			// (2)入力画像のチャンネル数分の画像領域を確保
			sch = src_img1.NChannels;
			for (i = 0; i < sch; i++)
			{
				dst_img1[i] = Cv.CreateImage(Cv.Size(src_img1.Width, src_img1.Height), src_img1.Depth, 1);
			}

			// (3)ヒストグラム構造体を確保
			int[] nHisSize = new int[1];
			nHisSize[0] = 256;
			hist1[0] = Cv.CreateHist(nHisSize, HistogramFormat.Array, ranges, true);

			// (4)入力画像がマルチチャンネルの場合，画像をチャンネル毎に分割
			if (sch == 1)
			{
				Cv.Copy(src_img1, dst_img1[0]);
			}
			else
			{
				Cv.Split(src_img1, dst_img1[0], dst_img1[1], dst_img1[2], dst_img1[3]);
			}

			for (i = 0; i < sch; i++)
			{
				Cv.CalcHist(dst_img1[i], hist1[i], false);
				Cv.NormalizeHist(hist1[i], 10000);
				if (i < 3)
				{
					Cv.CopyHist(hist1[i], ref hist1[i + 1]);
				}
			}

			Cv.ReleaseImage(src_img1);

			foreach (string file in tempFiles)
			{
				try
				{
					dist = 0.0;

					src_img2 = IplImage.FromFile(file, LoadMode.AnyDepth | LoadMode.AnyColor);

					// (2)入力画像のチャンネル数分の画像領域を確保
					//sch = src_img1.NChannels;
					for (i = 0; i < sch; i++)
					{
						dst_img2[i] = Cv.CreateImage(Cv.Size(src_img2.Width, src_img2.Height), src_img2.Depth, 1);
					}

					// (3)ヒストグラム構造体を確保
					nHisSize[0] = 256;
					hist2 = Cv.CreateHist(nHisSize, HistogramFormat.Array, ranges, true);

					// (4)入力画像がマルチチャンネルの場合，画像をチャンネル毎に分割
					if (sch == 1)
					{
						Cv.Copy(src_img2, dst_img2[0]);
					}
					else
					{
						Cv.Split(src_img2, dst_img2[0], dst_img2[1], dst_img2[2], dst_img2[3]);
					}

					// (5)ヒストグラムを計算，正規化して，距離を求める
					for (i = 0; i < sch; i++)
					{
						Cv.CalcHist(dst_img2[i], hist2, false);
						Cv.NormalizeHist(hist2, 10000);
						tmp = Cv.CompareHist(hist1[i], hist2, HistogramComparison.Bhattacharyya);
						dist += tmp * tmp;
					}
					dist = Math.Sqrt(dist);

					// (6)求めた距離を文字として画像に描画
					Console.WriteLine("{0} => Distance={1:F3}", file, dist);

					Cv.ReleaseHist(hist2);
					Cv.ReleaseImage(src_img2);
				}
				catch (OpenCVException ex)
				{
					Console.WriteLine("Error : " + ex.Message);
				}
			}

			Cv.ReleaseHist(hist1[0]);
			Cv.ReleaseHist(hist1[1]);
			Cv.ReleaseHist(hist1[2]);
			Cv.ReleaseHist(hist1[3]);
		}
	}
}
