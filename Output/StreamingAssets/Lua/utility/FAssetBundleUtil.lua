
local l_instance = nil
local FAssetBundleUtil = FLua.Class("FAssetBundleUtil")
do
	function FAssetBundleUtil:_ctor()
		self.m_AssetsMgr = nil
	end
	function FAssetBundleUtil.Instance()
		if not l_instance then
			l_instance = FAssetBundleUtil.new()
		end
		return l_instance
	end
	function FAssetBundleUtil:InitAssetBundle()
		self.m_AssetsMgr = ResourceManager.Instance
		self.m_AssetsMgr:TouchInstance()
	end

	function FAssetBundleUtil:AsyncLoad(assetBundleName,assetName,cb)
		print("LoadAssetBundle:" .. assetBundleName)
		self.m_AssetsMgr:LoadAsset(assetBundleName,assetName,function(obj)
			if cb then cb(obj) end
		end)
	end

	function FAssetBundleUtil:UnloadAssetBundle(assetBundleName)
		if not assetBundleName or assetBundleName:len() == 0 then 
			return
		end
		if ResourceManager.IsAsyncMode then
			ResourceManager.UnloadAssetBundle(assetBundleName)
		else
			self.m_AssetsMgr:UnloadAssetBundle(assetBundleName)
		end
	end
end

return FAssetBundleUtil