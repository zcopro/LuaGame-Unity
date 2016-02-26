local FPlayer = require "player.FPlayer"
local FHostPlayer = FLua.Class("FHostPlayer",FPlayer)

do
	function FHostPlayer:_ctor()

	end

	function FHostPlayer:Init(info)
		self.m_IsHost = true
		self.m_InfoData = info
		self:Create("Villager_B_Boy_prefab")
	end

	function FHostPlayer:OnLoaded(obj)
		obj.tag = "HostPlayer"
		self:Play("Idle",UnityEngine.WrapMode.Loop)
		--theGame.m_MainCam:GetComponent("FSmootFollow").target = obj.transform
		self.m_IsReady = true
	end
end


return FHostPlayer
