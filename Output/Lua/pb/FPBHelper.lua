require "pb.Share_Common"

local l_c2sId2PbClass = {}
local l_s2cId2PbClass = {}
local l_nameToInfo = {}		--name => {type=type, name=name, pb_class=pb_class, id=id}
local l_pbClassToInfo = {}		--pb_class => {type=type, name=name, pb_class=pb_class, id=id}

do
	local function registerC2S(type,name, id, pb_class)
		local info = {type=type, name=name, pb_class=pb_class, id=id}
		l_nameToInfo[name] = info
		l_pbClassToInfo[pb_class] = info
		l_c2sId2PbClass[id] = pb_class
	end
	local function registerS2C(type,name, id, pb_class)
		local info = {type=type, name=name, pb_class=pb_class, id=id}
		l_nameToInfo[name] = info
		l_pbClassToInfo[pb_class] = info
		l_s2cId2PbClass[id] = pb_class
	end


	local C2S_PROTO_TYPE = Share_Common.C2S_PROTO_TYPE	--C2S协议编号
	local S2C_PROTO_TYPE = Share_Common.S2C_PROTO_TYPE	--S2C协议编号

	for MsgName, MsgType in pairs(Share_Common) do
		if type(MsgType) == "table" and MsgType.GetFieldDescriptor then	--是一个 protocol buffer 消息
			local field = MsgType.GetFieldDescriptor("type_t")
			if field then
				local theType = field.enum_type
				local MsgID = field.default_value
				if theType == C2S_PROTO_TYPE then
					registerC2S(theType,MsgName, MsgID, MsgType)
				elseif theType == S2C_PROTO_TYPE then
					registerS2C(theType,MsgName, MsgID, MsgType)
				end
			end
		end
	end
end

local FPBHelper = FLua.Class("FPBHelper")
do
	function FPBHelper.GetPbClass(name)
		local info = l_nameToInfo[cmdName]
		if info then
			return info.pb_class
		else
			return nil
		end
	end
	function FPBHelper.NewPbMsg(name)
		local info = l_nameToInfo[cmdName]
		if info then
			return info.pb_class()
		else
			return nil
		end
	end
	function FPBHelper.GetPbId(pb_class)
		local info = l_pbClassToInfo[pb_class]
		if info then
			return info.id
		else
			return nil
		end
	end
	function FPBHelper.GetPbName(pb_class)
		local info = l_pbClassToInfo[pb_class]
		if info then
			return info.name
		else
			return nil
		end
	end
	function FPBHelper.GetPbClass(id)
		return l_c2sId2PbClass[id]
	end
	
end

return FPBHelper

