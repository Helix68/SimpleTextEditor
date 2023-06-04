using System.Numerics;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;

namespace SimpleTextEditor;

internal static class Program
{
	[STAThread]
	static void Main()
	{
		SimpleTextEditorApp app = new SimpleTextEditorApp();
		app.RunMainLoop();
		app.Cleanup();
	}
}

class SimpleTextEditorApp
{
	bool _needsExit;
	Form _form;

	ID2D1Factory _factoryD2D1;
	IDWriteFactory _factoryDWrite;

	ID2D1HwndRenderTarget _renderTarget;

	ID2D1SolidColorBrush _solidColorBrush;

	IDWriteTextFormat _textFormat;
	IDWriteTextLayout _textLayout;

	public SimpleTextEditorApp()
	{
		ApplicationConfiguration.Initialize();

		_form = new Form()
		{
			Text = "Simple Text Editor",
			StartPosition = FormStartPosition.CenterScreen,
			Width = 1300,
			Height = 700
		};

		_factoryD2D1 = D2D1.D2D1CreateFactory<ID2D1Factory>();
		_factoryDWrite = DWrite.DWriteCreateFactory<IDWriteFactory>();

		RenderTargetProperties props = new RenderTargetProperties(
			RenderTargetType.Hardware,
			new Vortice.DCommon.PixelFormat(Vortice.DXGI.Format.Unknown, Vortice.DCommon.AlphaMode.Premultiplied),
			96.0f, 96.0f,
			RenderTargetUsage.None,
			FeatureLevel.Default);
		HwndRenderTargetProperties hwndProps = new HwndRenderTargetProperties()
		{
			Hwnd = _form.Handle,
			PixelSize = _form.ClientSize,
			PresentOptions = PresentOptions.Immediately
		};
		_renderTarget = _factoryD2D1.CreateHwndRenderTarget(props, hwndProps);

		_solidColorBrush = _renderTarget.CreateSolidColorBrush(new Color4(0, 0, 0));

		_textFormat = _factoryDWrite.CreateTextFormat("Cascadia Mono Light", 40.0f);
		_textLayout = _factoryDWrite.CreateTextLayout("Simple Text Editor", _textFormat, float.MaxValue, float.MaxValue);

		_form.FormClosed += Form_FormClosed;
		_form.KeyDown += Form_KeyDown;
		_form.ClientSizeChanged += _form_ClientSizeChanged;
	}

	private void _form_ClientSizeChanged(object? sender, EventArgs e)
	{
		if (_renderTarget == null)
		{
			return;
		}

		_renderTarget.Resize(_form.ClientSize);
		RenderForm();
	}

	public void RunMainLoop()
	{
		_form.Show();

		while (!_needsExit)
		{
			Application.DoEvents();
			RenderForm();
		}
	}

	public void Cleanup()
	{
		_factoryDWrite?.Release();
		_factoryD2D1?.Release();
		_renderTarget?.Release();
	}

	private void Form_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Escape)
		{
			_needsExit = true;
			Application.Exit();
		}
	}

	private void Form_FormClosed(object? sender, FormClosedEventArgs e)
	{
		_needsExit = true;
		Application.Exit();
	}

	private void RenderForm()
	{
		if (_renderTarget == null)
		{
			return;
		}

		_renderTarget.BeginDraw();
		_renderTarget.Clear(Colors.White);

		_renderTarget.DrawTextLayout(new Vector2(0, 0), _textLayout, _solidColorBrush);

		_renderTarget.EndDraw();
	}
}

