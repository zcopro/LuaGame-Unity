
local l_instance = nil
local FGUIMan = FLua.Class("FGUIMan")
do
	function FGUIMan.Instance()
		if not l_instance then
			l_instance = FGUIMan.new()
		end
		return l_instance
	end
	function FGUIMan:_ctor()
		self.m_UIRoot = nil
		self.m_PanelContainer = {}
	end

	function FGUIMan:GetGUIRoot()
		return self.m_UIRoot
	end

	function FGUIMan:InitUIRoot()
		if self.m_UIRoot then return end
		local goRoot = NewGameObject("UIRoot(2D)");
	    goRoot.transform.localPosition = Vector3(0, 0, 0);
	    goRoot.transform.localScale = Vector3(1, 1, 1);
	    goRoot.layer = UnityEngine.LayerMask.NameToLayer("UI");

	    local cam = goRoot:AddComponent(UnityEngine.Camera)
	    cam.clearFlags = UnityEngine.CameraClearFlags.Depth
	    --cam.backgroundColor = Color(128,128,128,255)
	    cam.cullingMask = 32
	    cam.orthographic = true;
	    cam.orthographicSize = 3.2
	    cam.nearClipPlane = -10;
	    cam.farClipPlane = 1000;

	    goRoot:AddComponent(UnityEngine.FlareLayer);
        goRoot:AddComponent(LuaHelper.GetClsType("UnityEngine.GUILayer"));

        local goCanvas = NewGameObject("Canvas")
        goCanvas.layer = UnityEngine.LayerMask.NameToLayer("UI");
        goCanvas.transform:SetParent(goRoot.transform)
	    goCanvas.transform.localPosition = Vector3(0, 0, 0);
	    goCanvas.transform.localScale = Vector3(1, 1, 1);
	    local canvas = goCanvas:AddComponent(UnityEngine.Canvas);
	    canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera;
	    canvas.pixelPerfect = true
	    canvas.worldCamera = cam

	    local canScaler = goCanvas:AddComponent(UnityEngine.UI.CanvasScaler);
	    canScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize
	    canScaler.referenceResolution = Vector2(960,640)

	    goCanvas:AddComponent(UnityEngine.UI.GraphicRaycaster);

        --Event Handle
		local goEvent = NewGameObject("EventSystem");
	    goEvent:AddComponent(EventSystems.EventSystem);
	    goEvent:AddComponent(EventSystems.StandaloneInputModule);
	    --goEvent:AddComponent(EventSystems.TouchInputModule);
	    DontDestroyOnLoad(goEvent)
	    goEvent.transform:SetParent(goRoot.transform)


	    self.m_UIRoot = goCanvas.transform
	end

	function FGUIMan:RegisterPanel(name,panel)
		self.m_PanelContainer[name] = panel
	end

end

return FGUIMan