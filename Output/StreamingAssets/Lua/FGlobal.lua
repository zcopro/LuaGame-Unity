import "UnityEngine"

ResourceManager = FGame.Manager.ResourceManager
NetworkManager = FGame.Manager.NetworkManager
ByteBuffer = FGame.Common.ByteBuffer
LuaBehaviour = FGame.UI.LuaBehaviour

GameObject = UnityEngine.GameObject
Vector2 = UnityEngine.Vector2
Vector3 = UnityEngine.Vector3
Quaternion = UnityEngine.Quaternion

function OnUnityLog(t,str)
	local game = require "game.FGame"
	game.Instance():OnUnityLog(t,str)
	--theGame:OnUnityLog(t,str)
end

function OnHotKeyCodeMap()
	return {
		KeyCode.Tab,
		KeyCode.Escape,
		KeyCode.UpArrow,
		KeyCode.DownArrow,
		KeyCode.RightArrow,
		KeyCode.LeftArrow,
		KeyCode.Return,
	}
end

function OnHotKeyInput( key, down )
	local KeyCode = UnityEngine.KeyCode
	if key == KeyCode.Tab and down then
		theGame:ToggleConsole()
	elseif key == KeyCode.Escape then
		theGame:ForceCloseConsole()
	elseif key == KeyCode.UpArrow then
		if theGame.m_HostPlayer then
			theGame.m_HostPlayer:Play("Run",UnityEngine.WrapMode.Loop)
		end
	elseif key == KeyCode.DownArrow then
		if theGame.m_HostPlayer then
			theGame.m_HostPlayer:Play("Skill",UnityEngine.WrapMode.Once)
		end
	elseif key == KeyCode.RightArrow and down then
		if theGame.m_HostPlayer then
			theGame.m_HostPlayer:Play("Skill",UnityEngine.WrapMode.Once)
		end
	elseif key == KeyCode.Return then
		if theGame.m_HostPlayer then
			theGame.m_HostPlayer:Play("Idle",UnityEngine.WrapMode.Loop)
		end
	end
end

function OnReceiveMessage(protocal,buffer)
	local FNetwork = require "utility.FNetwork"
	FNetwork.Instance():OnReceiveMessage(protocal,buffer)
end

function AsyncLoad(assetBundleName,assetName,cb)
	local FAssetBundleUtil = require "utility.FAssetBundleUtil"
	FAssetBundleUtil.Instance():AsyncLoad(assetBundleName,assetName,cb)
end

function MsgBox(hwnd,content,title,mask,click_cb)
	local FMsgBoxMan = require "manager.FMsgBoxMan"
	FMsgBoxMan.Instance():ShowMsgBox(hwnd,content,title,mask,click_cb)
end

function AddEvent(obj, ...)
	theGame.m_LogicEvent:AddEvent(obj,...)
end

function DelEvent(obj,...)
	theGame.m_LogicEvent:DelEvent(obj,...)
end

function FireEvent(eventname,...)
	theGame.m_LogicEvent:Fire(eventname,...)
end

function NewGameObject(name)
	if type(name) == "string" then
		return GameObject(name)
	elseif not name then
		return GameObject()
	else
		error("constructor GameObject param 1 is expected nil or string")
	end
end

function Instantiate(go,name)
	local obj = UnityEngine.Object.Instantiate(go)
	if type(name) == "string" then
		obj.name = name
	end
	return obj
end

function DestroyObject(go)
	UnityEngine.Object.Destroy(go)
end

function DontDestroyOnLoad(go)
	UnityEngine.Object.DontDestroyOnLoad(go)
end

function Table2String(tab)
    local str = {}
    local function internal(tab, str, indent)
        for k,v in pairs(tab) do
            if type(v) == "table" then
                table.insert(str, indent..tostring(k)..":\n")
                internal(v, str, indent..' ')
            else
                table.insert(str, indent..tostring(k)..": "..tostring(v).."\n")
            end
        end
    end

    internal(tab, str, '')
    return table.concat(str, '')
end

function string:split(sep)
	local sep, fields = sep or ",", {}
	local pattern = string.format("([^%s]+)", sep)
	self:gsub(pattern, function(c) table.insert(fields, c) end)
	return fields
end

function NewByteBuffer(data)
	if not data then
		return ByteBuffer()
	else
		return ByteBuffer(data)
	end
end
