
theGame = nil
local FNetwork = require "utility.FNetwork"
local FAssetBundleUtil = require "utility.FAssetBundleUtil"
local FLibEvent = require "utility.FLibEvent"
local FConsoleUI = require "ui.FConsoleUI"
local FCommand = require "game.FCommand"

local FGame = FLua.Class("FGame")
do
	function FGame.Instance( )
		return theGame
	end

	function FGame:_ctor()
		self.m_Network = nil
		self.m_AssetBundle = nil
		self.m_LogicEvent = FLibEvent.new("LogicEvent")
		self.m_LogList = {}
		self.m_MainCam = nil
		self.m_HostPlayer = nil
		self.m_FPS = nil
		self.m_LoginInfo = nil
		self.m_isGameLogic = false
	end

	function FGame:InitGame()
		--GameManager
		self.m_Network = FNetwork.Instance()
		self.m_Network:InitNetwork()
		self.m_AssetBundle = FAssetBundleUtil.Instance()
		self.m_AssetBundle:InitAssetBundle()
		self:InitGameObject()
		--Init GUIRoot
		local FGUIMan = require "ui.FGUIMan"
		FGUIMan.Instance():InitUIRoot()
		local FFlashTipMan = require "manager.FFlashTipMan"
		FlashTipMan:InitCacheRoot()
	end

	function FGame:InitLoginInfo()
		self.m_LoginInfo = {name="libyyu",passwd="123456",}
	end

	function FGame:InitGameObject()
		--Main Camera
		local cam_root = NewGameObject("MainCamera Root")
	    cam_root.transform.localPosition = Vector3(0, 0, 0);
	    cam_root.transform.localScale = Vector3(1, 1, 1);
	    local camobj = NewGameObject("Main Camera")
	    camobj.transform:SetParent(cam_root.transform)
	    camobj.transform.localPosition = Vector3(85, 18, 20);
	    camobj.transform.localRotation = Quaternion.New(0.1, -0.9, 0.4, 0.2);
	    camobj.transform.localScale = Vector3(1, 1, 1);
	    camobj:AddComponent(LuaHelper.GetClsType("FSmootFollow"))
	    camobj:AddComponent(UnityEngine.Camera)
	    camobj:AddComponent(UnityEngine.FlareLayer)
        camobj:AddComponent(LuaHelper.GetClsType("UnityEngine.GUILayer"));
        --camobj:AddComponent(UnityEngine.AudioListener)
        camobj.tag = "MainCamera"
        self.m_MainCam = camobj
	    DontDestroyOnLoad(cam_root)
	    --AudioListener
	    local goAudio = NewGameObject("AudioListener")
	    goAudio:AddComponent(UnityEngine.AudioListener)
	    DontDestroyOnLoad(goAudio)
		--Init FHotKeyLogic
		local hotGo = NewGameObject("FHotKeyLogic")
		local clsT = LuaHelper.GetClsType("FHotKeyLogic")
		hotGo:AddComponent(clsT)
		DontDestroyOnLoad(hotGo)
	end

	function FGame:Run()
		self:InitGame()

		self:EnterLoginStage()
	end
	function FGame:EnterLoginStage()
		self:InitLoginInfo()
		local login = require "ui.FLoginUI"
		login.Instance():ShowPanel(true)
	end
	function FGame:LeaveLoginState()
		local login = require "ui.FLoginUI"
		login.Instance():ShowPanel(false)
	end

	function FGame:EnterGameLogic()
		self:LeaveLoginState()
		self.m_isGameLogic = true
		--加载世界
		AsyncLoad("Map","x1",function(asset)
			Instantiate(asset)

			local player = require "player.FHostPlayer"
			local p = player.new()
			p:Init({})
			self.m_HostPlayer = p
		end)
	end
	function FGame:LeaveGameLogic()
		self.m_isGameLogic = false
	end


	function FGame:ToggleConsole()
		FConsoleUI.Instance():ToggleConsole()
	end
	function FGame:ForceCloseConsole()
		FConsoleUI.Instance():DestroyPanel()
	end
	function FGame:OnUnityLog(t,str)
		table.insert(self.m_LogList,{type=t,str=str})
		FireEvent(EventDef.UnityLog,{type=t,str=str})
	end
	function FGame:GetAllLogs()
		return self.m_LogList
	end

	function FGame:ExecuteDebugString(input)
		local args = string.split(input, ' ')
		local cmd = table.remove(args,1)

		local commands = FCommand:getAllCommands()
		if commands[cmd] then
			commands[cmd].execute(args)
		else
			warn("unknow command:"..cmd)
		end
	end

	function FGame:ShowFPS(show)
		if show then
			if not self.m_FPS then
				self.m_FPS = NewGameObject("FPS")
			end
			local fps = self.m_FPS:GetComponent("FPS")
			if not fps then
				self.m_FPS:AddComponent(LuaHelper.GetClsType("FPS"))
			end
		else
			if self.m_FPS then
				DestroyObject(self.m_FPS)
				self.m_FPS = nil
			end
		end
	end
end

theGame = FGame.new()

return FGame
