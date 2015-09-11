using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MouseTest1
{
	/// <summary>
	/// 画像の色は24bitで
	/// </summary>
	class ImageSearch
	{
		//アクティブウィンドウ
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		[DllImport("User32.Dll")]
		static extern int GetWindowRect(IntPtr hWnd, out RECT rect);

		[DllImport("user32.dll")]
		extern static IntPtr GetForegroundWindow();

		public ImageSearch()
		{

		}

		public void getActiveWindowImage()
		{

			RECT r;
			IntPtr active = GetForegroundWindow();
			GetWindowRect(active, out r);
			Rectangle rect = new Rectangle(r.left, r.top, r.right - r.left, r.bottom - r.top);

			Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);

			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
			}

			Bitmap bmp_src = new Bitmap(@"E:\user\Documents\GitHub\Grabl_BOT\Image\src\test.bmp");
			Bitmap bmp_ptn = new Bitmap(@"E:\user\Documents\GitHub\Grabl_BOT\Image\ptn\D9.bmp");

			//ソース画像から45x45の切り抜き
			Rectangle rect_ptn1 = new Rectangle(36, 221, 45, 45);
			Bitmap bmp_src1 = bmp_src.Clone(rect_ptn1, bmp_src.PixelFormat);

			//Point p = SearchImage32(bmp_src , bmp_ptn);
			//Debug.WriteLine(p.X + "," + p.Y);

			//OpenCVでヒストグラム類似度計算
			testCV(@"E:\user\Documents\GitHub\Grabl_BOT\Image\ptn",bmp_ptn);

			//TODO 場所指定
			//bmp.Save(@"E:\user\Documents\GitHub\Grabl_BOT\Image\src_test1.bmp", ImageFormat.Bmp);
			//TODO 場所指定
			//bmp_ptn.Save(@"E:\user\Documents\GitHub\Grabl_BOT\Image\ptn_test1.bmp", ImageFormat.Bmp);

			bmp.Dispose();
			bmp_src.Dispose();
			bmp_ptn.Dispose();
			bmp_src1.Dispose();
		}

		public void createPatternFile()
		{
			RECT r;
			IntPtr active = GetForegroundWindow();
			GetWindowRect(active, out r);
			Rectangle rect = new Rectangle(r.left, r.top, r.right - r.left, r.bottom - r.top);

			Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);

			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
			}

			//ソース画像から45x45の切り抜き
			Rectangle rect_ptn = new Rectangle(260, 221, 45, 45);
			Bitmap bmp_src1 = bmp.Clone(rect_ptn, bmp.PixelFormat);

			bmp.Save(@"E:\user\Documents\GitHub\Grabl_BOT\Image\Uncategorize\test_src.bmp", ImageFormat.Bmp);
			bmp_src1.Save(@"E:\user\Documents\GitHub\Grabl_BOT\Image\Uncategorize\test_ptn.bmp", ImageFormat.Bmp);

			bmp.Dispose();
			bmp_src1.Dispose();

		}

		/// <summary>
		/// OpnenCVでやってみる版
		/// </summary>
		public void testCV(String ptn,Bitmap src)
		{
			int i, sch = 0;
			float[] range_0 = { 0, 256 };
			float[][] ranges = { range_0 };
			double tmp, dist = 0;
			IplImage src_img1, src_img2;
			IplImage[] dst_img1 = new IplImage[4];
			IplImage[] dst_img2 = new IplImage[4];

			CvHistogram[] hist1 = new CvHistogram[4];
			CvHistogram hist2;

			//templateフォルダ以下にテンプレート画像を入れておく。
			IEnumerable<string> tempFiles = Directory.EnumerateFiles(ptn, "*.bmp", SearchOption.TopDirectoryOnly);
			Dictionary<string, double> NCCPair = new Dictionary<string, double>();

			//(1)二枚の画像を読み込む．チャンネル数が等しくない場合は，終了
			//src_img1 = IplImage.FromFile(args[0], LoadMode.AnyDepth | LoadMode.AnyColor);
			src_img1 = (OpenCvSharp.IplImage)BitmapConverter.ToIplImage(src);

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
					Debug.WriteLine("{0} => Distance={1:F3}", file, dist);

					Cv.ReleaseHist(hist2);
					Cv.ReleaseImage(src_img2);
				}
				catch (OpenCVException ex)
				{
					Debug.WriteLine("Error : " + ex.Message);
				}
			}

			Cv.ReleaseHist(hist1[0]);
			Cv.ReleaseHist(hist1[1]);
			Cv.ReleaseHist(hist1[2]);
			Cv.ReleaseHist(hist1[3]);
		}

		/// <summary>
		/// 手動でパターンマッチング（完全に一致しないと駄目）
		/// </summary>
		/// <param name="src"></param>
		/// <param name="ptn"></param>
		/// <returns></returns>
		private Point SearchImage32(Bitmap src, Bitmap ptn)
		{
			BitmapData srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			BitmapData ptnData = ptn.LockBits(new Rectangle(0, 0, ptn.Width, ptn.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			byte[] srcPix, ptnPix, srcLine, ptnLine;
			srcPix = new byte[src.Width * src.Height * 4];
			ptnPix = new byte[ptn.Width * ptn.Height * 4];
			srcLine = new byte[ptn.Width * 4];
			ptnLine = new byte[ptn.Width * 4];
			Marshal.Copy(srcData.Scan0, srcPix, 0, srcPix.Length);
			Marshal.Copy(ptnData.Scan0, ptnPix, 0, ptnPix.Length);
			Point agreePoint = Point.Empty;
			bool agree = true;

			for (int y = 0; y < src.Height - ptn.Height; y++)
			{
				for (int x = 0; x < src.Width - ptn.Width; x++)
				{
					agree = true;
					for (int yy = 0; yy < ptn.Height; yy++)
					{
						System.Array.Copy(srcPix, (x + (yy + y) * src.Width) * 4, srcLine, 0, (srcLine.Length));
						System.Array.Copy(ptnPix, yy * ptn.Width * 4, ptnLine, 0, (ptnLine.Length));
						if (srcLine.SequenceEqual(ptnLine) == false) agree = false;
						if (agree == false) break;
					}
					if (agree)
					{
						agreePoint = new Point(x, y);
						break;
					}
				}
				if (agree) break;
			}
			src.UnlockBits(srcData);
			ptn.UnlockBits(ptnData);
			return agreePoint;
		}

		public Point SearchImage24(Bitmap src, Bitmap ptn)
		{
			BitmapData srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
			BitmapData ptnData = ptn.LockBits(new Rectangle(0, 0, ptn.Width, ptn.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
			byte[] srcPix, ptnPix, srcLine, ptnLine;
			srcPix = new byte[src.Width * src.Height * 3];
			ptnPix = new byte[ptn.Width * ptn.Height * 3];
			srcLine = new byte[ptn.Width * 3];
			ptnLine = new byte[ptn.Width * 3];
			Marshal.Copy(srcData.Scan0, srcPix, 0, srcPix.Length);
			Marshal.Copy(ptnData.Scan0, ptnPix, 0, ptnPix.Length);
			Point agreePoint = Point.Empty;
			bool agree = true;

			for (int y = 0; y < src.Height - ptn.Height; y++)
			{
				for (int x = 0; x < src.Width - ptn.Width; x++)
				{
					agree = true;
					for (int yy = 0; yy < ptn.Height; yy++)
					{
						System.Array.Copy(srcPix, (x + (yy + y) * src.Width) * 3, srcLine, 0, (srcLine.Length));
						System.Array.Copy(ptnPix, yy * ptn.Width * 3, ptnLine, 0, (ptnLine.Length));

						if (srcLine.SequenceEqual(ptnLine) == false) agree = false;
						if (agree == false) break;

					}
					if (agree)
					{
						agreePoint = new Point(x, y);
						break;
					}
				}
				if (agree) break;
			}
			src.UnlockBits(srcData);
			ptn.UnlockBits(ptnData);


			return agreePoint;
		}
	}
}
