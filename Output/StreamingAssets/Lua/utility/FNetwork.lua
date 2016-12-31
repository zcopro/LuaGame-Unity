
local l_instance = nil
local FNetwork = FLua.Class("FNetwork")
do
	function FNetwork:_ctor()
		self.m_Network = nil
		self.m_UserInfo = nil
		self.m_ip = ""
		self.m_port = 0
		self.m_status = "broken"
	end
	function FNetwork.Instance()
		if not l_instance then
			l_instance = FNetwork.new()
		end
		return l_instance
	end
	function FNetwork:InitNetwork()
		self.m_Network = NetworkManager.Instance
		self.m_Network:TouchInstance()
	end

	function FNetwork:Connect()
		self.m_status = "connecting"
		self.m_Network:ConnectTo(self.m_ip,self.m_port)
	end

	function FNetwork:ConnectTo(ip,port,name,passwd)
		self.m_ip = ip
		self.m_port = port
		self.m_UserInfo = {name=name,passwd=passwd,}
		self:Connect()
	end

	function FNetwork:isConnected()
		return self.m_status == "connected" and self.m_Network and not self.m_Network.isNil
	end

	function FNetwork:Close()
		self.m_Network:Close()
		self.m_status = "broken"
	end

	function FNetwork:Ping(ip)
		self.m_Network:Ping(ip)
	end

	function FNetwork:Send(buffer)
		return self.m_Network:SendMessage(buffer)
	end

	function FNetwork:SendPB(pb_msg)
		local FPBHelper = require "pb.FPBHelper"
		local pb_class = pb_msg:GetMessage()
		local id = FPBHelper.GetPbId(pb_class)
		if id then
			local msgbuf = pb_msg:SerializeToString();
			local count = self.m_Network:SendPbMessage(msgbuf)
		    warn("send bytes-count:",count, ", content:", pb_msg)

		    local buffer = NewByteBuffer()
		    buffer:WriteBytesString(msgbuf)
		    local bytes = buffer:ToBytes()
		    warn("Send bytes:", GameUtil.ToBytesString(bytes))
		else
			warn("Can not GetPbId pb_class:",pb_class)
		end
	end

	function FNetwork:OnConnected()
		warn("FNetwork:OnConnected")
		self.m_status = "connected"
		local name = self.m_UserInfo.name
		local passwd = self.m_UserInfo.passwd
		local msg = Share_Common.Stuff_Account()
		--msg.type_t = Share_Common.Proto_Stuff_Account
		msg.UserName = name
		msg.PassWord = passwd
		self:SendPB(msg)
	end

	function FNetwork:OnTimeout()
		warn("FNetwork:OnTimeout")
		self.m_status = "disconnect"
	end

	function FNetwork:OnDisconnect(reason, err_msg)
		warn("FNetwork:OnDisconnect reason="..reason .. ",err_msg="..err_msg)
		self.m_status = reason
		local content = reason == "broken" and StringReader.Get(1) or StringReader.Get(2)
		MsgBox(self,content,reason,MsgBoxType.MBBT_OKCANCEL,function(_,ret)
			if ret == MsgBoxRetT.MBRT_OK then
				self:Connect()
			end
		end)

		--local FLoadingUI = require "ui.FLoadingUI"
		--FLoadingUI.Instance():ShowPanel(true)
	end

	function FNetwork:OnPing(buffer)
		local text = buffer:ReadString()
		warn(text)
	end

	function FNetwork:OnReceiveMessage(protocal,buffer)
		local Protocal = FGame.Manager.Protocal
		if protocal == Protocal.Connect then
			self:OnConnected()
		elseif protocal == Protocal.Exception then
			self:OnDisconnect("broken", buffer:ReadString())
		elseif protocal == Protocal.Disconnect then
			self:OnDisconnect("disconnect", buffer:ReadString())
		elseif protocal == Protocal.Timeout then
			self:OnTimeout()
		elseif protocal == Protocal.Ping then
			self:OnPing(buffer)
		elseif protocal == Protocal.GameData then
			self:OnGameData(buffer)
		end
	end

	function FNetwork:OnGameData(buffer)
		warn("FNetwork:OnGameData",buffer)
		local id = buffer:ReadShort()
		local FPBHelper = require "pb.FPBHelper"
		local pb_class = FPBHelper.GetPbClass(id)
		if pb_class then
			local msg = pb_class()
			msg:ParseFromString(buffer:ReadeBytesString())
			FireEvent(FPBHelper.GetPbName(pb_class),msg)
			warn("Receive PB:",pb_class,msg)
		else
			warn("unknow msg id:"..id)
		end
	end
end

return FNetwork