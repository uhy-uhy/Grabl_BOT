using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MouseTest1
{
	public partial class Form1 : Form
	{
		// Watcher
		private OrderdKeyWatcher _watcher;
					
		private ImageSearch _ims;


		public Form1()
		{
			InitializeComponent();

			_ims = new ImageSearch();

			// キーボードを監視する
			_watcher = new OrderdKeyWatcher(50, 1000, callBackMethod, (int)Keys.LShiftKey, (int)Keys.LShiftKey);
			_watcher.Watch();
		}


		// マウスイベント(mouse_eventの引数と同様のデータ)
		[StructLayout(LayoutKind.Sequential)]
		private struct MOUSEINPUT
		{
			public int dx;
			public int dy;
			public int mouseData;
			public int dwFlags;
			public int time;
			public int dwExtraInfo;
		};

		// キーボードイベント(keybd_eventの引数と同様のデータ)
		[StructLayout(LayoutKind.Sequential)]
		private struct KEYBDINPUT
		{
			public short wVk;
			public short wScan;
			public int dwFlags;
			public int time;
			public int dwExtraInfo;
		};

		// ハードウェアイベント
		[StructLayout(LayoutKind.Sequential)]
		private struct HARDWAREINPUT
		{
			public int uMsg;
			public short wParamL;
			public short wParamH;
		};

		// 各種イベント(SendInputの引数データ)
		[StructLayout(LayoutKind.Explicit)]
		private struct INPUT
		{
			[FieldOffset(0)]
			public int type;
			[FieldOffset(4)]
			public MOUSEINPUT mi;
			[FieldOffset(4)]
			public KEYBDINPUT ki;
			[FieldOffset(4)]
			public HARDWAREINPUT hi;
		};

		// キー操作、マウス操作をシミュレート(擬似的に操作する)
		[DllImport("user32.dll")]
		private extern static void SendInput(
			int nInputs, ref INPUT pInputs, int cbsize);

		// 仮想キーコードをスキャンコードに変換
		[DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
		private extern static int MapVirtualKey(
			int wCode, int wMapType);

		private const int INPUT_MOUSE = 0;                  // マウスイベント
		private const int INPUT_KEYBOARD = 1;               // キーボードイベント
		private const int INPUT_HARDWARE = 2;               // ハードウェアイベント

		private const int MOUSEEVENTF_MOVE = 0x1;           // マウスを移動する
		private const int MOUSEEVENTF_ABSOLUTE = 0x8000;    // 絶対座標指定
		private const int MOUSEEVENTF_LEFTDOWN = 0x2;       // 左　ボタンを押す
		private const int MOUSEEVENTF_LEFTUP = 0x4;         // 左　ボタンを離す
		private const int MOUSEEVENTF_RIGHTDOWN = 0x8;      // 右　ボタンを押す
		private const int MOUSEEVENTF_RIGHTUP = 0x10;       // 右　ボタンを離す
		private const int MOUSEEVENTF_MIDDLEDOWN = 0x20;    // 中央ボタンを押す
		private const int MOUSEEVENTF_MIDDLEUP = 0x40;      // 中央ボタンを離す
		private const int MOUSEEVENTF_WHEEL = 0x800;        // ホイールを回転する
		private const int WHEEL_DELTA = 120;                // ホイール回転値

		private const int KEYEVENTF_KEYDOWN = 0x0;          // キーを押す
		private const int KEYEVENTF_KEYUP = 0x2;            // キーを離す
		private const int KEYEVENTF_EXTENDEDKEY = 0x1;      // 拡張コード
		private const int VK_SHIFT = 0x10;                  // SHIFTキー

		// マウスの右ボタンをクリックする
		//private void button1_Click(object sender, EventArgs e)
		private void right_Click()
		{
			// 自ウインドウを非表示(マウス操作対象のウィンドウへフォーカスを移動するため)
			this.Visible = false;

			// マウス操作実行用のデータ
			const int num = 3;
			INPUT[] inp = new INPUT[num];

			// (1)マウスカーソルを移動する(スクリーン座標でX座標=800ピクセル,Y=400ピクセルの位置)
			inp[0].type = INPUT_MOUSE;
			inp[0].mi.dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE;
			inp[0].mi.dx = 800 * (65535 / Screen.PrimaryScreen.Bounds.Width);
			inp[0].mi.dy = 400 * (65535 / Screen.PrimaryScreen.Bounds.Height);
			inp[0].mi.mouseData = 0;
			inp[0].mi.dwExtraInfo = 0;
			inp[0].mi.time = 0;
			
			// (2)マウスの右ボタンを押す
			inp[1].type = INPUT_MOUSE;
			inp[1].mi.dwFlags = MOUSEEVENTF_RIGHTDOWN;
			inp[1].mi.dx = 0;
			inp[1].mi.dy = 0;
			inp[1].mi.mouseData = 0;
			inp[1].mi.dwExtraInfo = 0;
			inp[1].mi.time = 0;

			// (3)マウスの右ボタンを離す
			inp[2].type = INPUT_MOUSE;
			inp[2].mi.dwFlags = MOUSEEVENTF_RIGHTUP;
			inp[2].mi.dx = 0;
			inp[2].mi.dy = 0;
			inp[2].mi.mouseData = 0;
			inp[2].mi.dwExtraInfo = 0;
			inp[2].mi.time = 0;

			// マウス操作実行
			SendInput(num, ref inp[0], Marshal.SizeOf(inp[0]));

			// 1000ミリ秒スリープ
			System.Threading.Thread.Sleep(1000);

			// 自ウインドウを表示
			this.Visible = true;
		}

		// マウスの左ボタンをダブルクリックする
		private void left_double_Click(object sender, EventArgs e)
		{
			// 自ウインドウを非表示(マウス操作対象のウィンドウへフォーカスを移動するため)
			this.Visible = false;

			// マウス操作実行用のデータ
			const int num = 5;
			INPUT[] inp = new INPUT[num];

			// (1)マウスカーソルを移動する(スクリーン座標でX座標=800ピクセル,Y=400ピクセルの位置)
			inp[0].type = INPUT_MOUSE;
			inp[0].mi.dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE;
			inp[0].mi.dx = 800 * (65535 / Screen.PrimaryScreen.Bounds.Width);
			inp[0].mi.dy = 400 * (65535 / Screen.PrimaryScreen.Bounds.Height);
			inp[0].mi.mouseData = 0;
			inp[0].mi.dwExtraInfo = 0;
			inp[0].mi.time = 0;

			// (2)マウスの左ボタンを押す
			inp[1].type = INPUT_MOUSE;
			inp[1].mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
			inp[1].mi.dx = 0;
			inp[1].mi.dy = 0;
			inp[1].mi.mouseData = 0;
			inp[1].mi.dwExtraInfo = 0;
			inp[1].mi.time = 0;

			// (3)マウスの左ボタンを離す
			inp[2].type = INPUT_MOUSE;
			inp[2].mi.dwFlags = MOUSEEVENTF_LEFTUP;
			inp[2].mi.dx = 0;
			inp[2].mi.dy = 0;
			inp[2].mi.mouseData = 0;
			inp[2].mi.dwExtraInfo = 0;
			inp[2].mi.time = 0;

			// (4)マウスの左ボタンを押す
			inp[3].type = INPUT_MOUSE;
			inp[3].mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
			inp[3].mi.dx = 0;
			inp[3].mi.dy = 0;
			inp[3].mi.mouseData = 0;
			inp[3].mi.dwExtraInfo = 0;
			inp[3].mi.time = 0;

			// (5)マウスの左ボタンを離す
			inp[4].type = INPUT_MOUSE;
			inp[4].mi.dwFlags = MOUSEEVENTF_LEFTUP;
			inp[4].mi.dx = 0;
			inp[4].mi.dy = 0;
			inp[4].mi.mouseData = 0;
			inp[4].mi.dwExtraInfo = 0;
			inp[4].mi.time = 0;

			// マウス操作実行
			SendInput(num, ref inp[0], Marshal.SizeOf(inp[0]));

			// 1000ミリ秒スリープ
			System.Threading.Thread.Sleep(1000);

			// 自ウインドウを表示
			this.Visible = true;
		}

		// イベント時に呼び出される
		private void callBackMethod(object sender, KeyWatcherEventArgs e)
		{
			Debug.WriteLine("shift２回");
			_ims.getActiveWindowImage();
			//_ims.createPatternFile();
		}
	}


}
