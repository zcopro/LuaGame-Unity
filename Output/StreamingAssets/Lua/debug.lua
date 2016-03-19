
-- GameUtil.SendRequest("http://www.baidu.com","hello",3,true,function(req,resp)
-- 	if resp.IsSuccess then
-- 		local str=resp.DataAsText
-- 		print(str)
-- 	else
-- 		print("error")
-- 	end
-- end)

-- GameUtil.DownLoad("http://www.baidu.com",
-- 	"E:/UnityWorks/t.txt",true,true,nil,function(a,b,c)
-- 		print("progress:",a,b,c)
-- 	end,function(req,resp)
-- 		print(req,resp)
-- 		if not req.Exception then
-- 			if resp.IsSuccess then
-- 				print("download completed.. now unzip")
-- 				--tmpfile=this.AssetRoot.."/tmp.zip"
-- 			end
-- 	    else 
-- 	    	print("DownLoad file err",resp)
-- 		end
-- 	end)

GameUtil.AnsyLoadLevel("StartScreen",function(success)
	print("LoadLevel:StartScreen",success)
end)