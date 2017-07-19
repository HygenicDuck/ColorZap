using UnityEngine;
using UnityEngine.UI;
using UI;
using Core;
using Utils;

namespace Utils
{
	public class DeviceSpecsWidget : BaseBehaviour
	{
		public const string PREFAB_NAME = "DeviceSpecsWidget";

		[SerializeField] private Text m_gpu;
		[SerializeField] private Text m_gpuBrand;
		[SerializeField] private Text m_cpu;
		[SerializeField] private Text m_model;
		[SerializeField] private Text m_memory;

		protected override void Awake()
		{
			Debugger.Log("SystemInfo.deviceModel " + SystemInfo.deviceModel, (int)SharedSystems.Systems.DEBUG_UTILS);
			Debugger.Log("SystemInfo.deviceName " + SystemInfo.deviceName, (int)SharedSystems.Systems.DEBUG_UTILS);
			Debugger.Log("SystemInfo.deviceType " + SystemInfo.deviceType, (int)SharedSystems.Systems.DEBUG_UTILS);
			Debugger.Log("SystemInfo.graphicsDeviceName " + SystemInfo.graphicsDeviceName, (int)SharedSystems.Systems.DEBUG_UTILS);
			Debugger.Log("SystemInfo.graphicsDeviceVendor " + SystemInfo.graphicsDeviceVendor, (int)SharedSystems.Systems.DEBUG_UTILS);
			Debugger.Log("SystemInfo.graphicsMemorySize " + SystemInfo.graphicsMemorySize, (int)SharedSystems.Systems.DEBUG_UTILS);
			Debugger.Log("SystemInfo.processorType " + SystemInfo.processorType, (int)SharedSystems.Systems.DEBUG_UTILS);
			Debugger.Log("SystemInfo.processorFrequency " + SystemInfo.processorFrequency, (int)SharedSystems.Systems.DEBUG_UTILS);
			Debugger.Log("SystemInfo.processorCount " + SystemInfo.processorCount, (int)SharedSystems.Systems.DEBUG_UTILS);
			Debugger.Log("SystemInfo.systemMemorySize " + SystemInfo.systemMemorySize, (int)SharedSystems.Systems.DEBUG_UTILS);

			m_gpu.text = SystemInfo.graphicsDeviceName;
			m_gpuBrand.text = SystemInfo.graphicsDeviceVendor;
			m_cpu.text = SystemInfo.processorType;
			m_model.text = SystemInfo.deviceModel;
			m_memory.text = "RAM: " + SystemInfo.systemMemorySize.ToString();
		}
	}
}
