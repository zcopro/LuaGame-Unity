
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
	    local canvas = goRoot:AddComponent(UnityEngine.Canvas);
	    canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera;
	    canvas.pixelPerfect = true

	    local canScaler = goRoot:AddComponent(UnityEngine.UI.CanvasScaler);
	    canScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize
	    canScaler.referenceResolution = Vector2(960,640)

	    goRoot:AddComponent(UnityEngine.UI.GraphicRaycaster);

	    local camobj = NewGameObject("GuiCamera")
	    camobj.layer = UnityEngine.LayerMask.NameToLayer("UI");
	    local cam = camobj:AddComponent(UnityEngine.Camera)
	    camobj.transform:SetParent(goRoot.transform)
	    camobj.transform.localPosition = Vector3(0, 0, 0);
	    camobj.transform.localScale = Vector3(1, 1, 1);
	    --cam.clearFlags = CameraClearFlags.Color
	    cam.clearFlags = UnityEngine.CameraClearFlags.Depth
	    --cam.backgroundColor = Color(128,128,128,255)
	    cam.cullingMask = 32
	    cam.orthographic = true;
	    cam.orthographicSize = 3.2
	    cam.nearClipPlane = -10;
	    cam.farClipPlane = 1000;

	    canvas.worldCamera = cam
	    camobj:AddComponent(UnityEngine.FlareLayer);
        camobj:AddComponent(LuaHelper.GetClsType("UnityEngine.GUILayer"));
        --camobj:AddComponent(UnityEngine.AudioListener);

	    self.m_UIRoot = camobj.transform
	end

	function FGUIMan:RegisterPanel(name,panel)
		self.m_PanelContainer[name] = panel
	end

end

return FGUIMan