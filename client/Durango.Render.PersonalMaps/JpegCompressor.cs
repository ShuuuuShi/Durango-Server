using System;
using System.IO;
using BitMiracle.LibJpeg.Classic;

namespace Durango.Render.PersonalMaps;

public class JpegCompressor
{
	public const int BytesPerPixel = 3;

	private readonly MemoryStream _memoryStream;

	private readonly byte[] _row;

	private readonly byte[][] _rowForDecompressor;

	private readonly jpeg_compress_struct m_compressor;

	private int _addRowCount;

	private JpegCompressor(int width, int height, int quality = 75, int smoothingFactor = 0, bool simpleProgressive = false)
	{
		_memoryStream = new MemoryStream();
		_row = new byte[width * 3];
		_rowForDecompressor = new byte[1][];
		_rowForDecompressor[0] = _row;
		m_compressor = new jpeg_compress_struct(new jpeg_error_mgr());
		m_compressor.Image_width = width;
		m_compressor.Image_height = height;
		m_compressor.In_color_space = J_COLOR_SPACE.JCS_RGB;
		m_compressor.Input_components = 3;
		m_compressor.jpeg_set_defaults();
		m_compressor.Smoothing_factor = smoothingFactor;
		m_compressor.jpeg_set_quality(quality, force_baseline: true);
		if (simpleProgressive)
		{
			m_compressor.jpeg_simple_progression();
		}
		m_compressor.jpeg_stdio_dest(_memoryStream);
		m_compressor.jpeg_start_compress(write_all_tables: true);
	}

	public static JpegCompressor Create(int width, int height, int quality = 75, int smoothingFactor = 0, bool simpleProgressive = false)
	{
		try
		{
			return new JpegCompressor(width, height, quality, smoothingFactor, simpleProgressive);
		}
		catch (Exception ex)
		{
			Debug.LogError("JpegCompressor.Create() failed: " + ex.Message);
			return null;
		}
	}

	public bool AddRow(byte[] bytes, int startIndex)
	{
		try
		{
			Array.Copy(bytes, startIndex, _row, 0, _row.Length);
			m_compressor.jpeg_write_scanlines(_rowForDecompressor, 1);
			_addRowCount++;
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("JpegCompressor.AddRow() failed: " + ex.Message);
			return false;
		}
	}

	public MemoryStream Finish()
	{
		try
		{
			if (_addRowCount == m_compressor.Image_height)
			{
				m_compressor.jpeg_finish_compress();
				return _memoryStream;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("JpegCompressor.Finish() failed: " + ex.Message);
		}
		Release();
		return null;
	}

	public void Release()
	{
		_memoryStream.Dispose();
	}
}
