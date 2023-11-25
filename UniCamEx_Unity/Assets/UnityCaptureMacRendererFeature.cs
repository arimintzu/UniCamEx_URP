using System;
using System.Collections;
using System.Collections.Generic;
using UniCamEx;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UnityCaptureMacRendererFeature : ScriptableRendererFeature
{
	[SerializeField]
	UnityCaptureMacSettings m_Settings = new UnityCaptureMacSettings();

	UnityCaptureRenderPass m_RenderPass;

	public UnityCaptureMacSettings Settings => m_Settings;

	public override void Create()
	{
		m_RenderPass = new UnityCaptureRenderPass(m_Settings)
		{
			renderPassEvent = RenderPassEvent.AfterRendering
		};
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
#if UNITY_EDITOR
		if (!Application.isPlaying || renderingData.cameraData.cameraType != CameraType.Game)
		{
			return;
		}
#endif
		renderer.EnqueuePass(m_RenderPass);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		m_RenderPass.Dispose();
	}
}

public class UnityCaptureRenderPass : ScriptableRenderPass, IDisposable
{

	UnityCaptureMacSettings m_Settings;
	RenderTexture m_RenderTexture;

	public UnityCaptureRenderPass(UnityCaptureMacSettings settings)
	{
		m_RenderTexture = new RenderTexture(settings.OutputResolusion.x, settings.OutputResolusion.y, 0, RenderTextureFormat.ARGB32);
		m_Settings = settings;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer cmd = CommandBufferPool.Get();

		RenderTargetIdentifier src = BuiltinRenderTextureType.CurrentActive;
		RenderTargetIdentifier dst = m_RenderTexture;

		cmd.Blit(src, dst);

		context.ExecuteCommandBuffer(cmd);

		UniCamExPlugin.Send(m_RenderTexture, m_Settings.FlipHorizontal);

		CommandBufferPool.Release(cmd);
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(m_RenderTexture);
		m_RenderTexture = null;
	}

}

[Serializable]
public class UnityCaptureMacSettings
{

	[SerializeField]
	Vector2Int m_OutputResolusion = new Vector2Int(1920, 1080);

	[SerializeField]
	bool m_FlipHorizontal = false;

	public Vector2Int OutputResolusion { get => m_OutputResolusion; set => m_OutputResolusion = value; }
	public bool FlipHorizontal { get => m_FlipHorizontal; set => m_FlipHorizontal = value; }
}
