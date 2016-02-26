local FTask = FLua.Class("FTask")
do
    function FTask:_ctor()
        self.next_co = nil, --活动队列中的下一个协程对象
        self.co = nil,      --协程对象
        self.status = nil,  --当前的状态
        self.block = nil,   --阻塞结构
        self.timeout = nil,
        self.index = 0,
        self.sc = nil,      --归属的调度器
        self.name = nil,
    end
    function FTask:init(name,sc,co)
        self.name = name
        self.sc = sc
        self.co = co
    end
    function FTask:Signal(ev)
        if self.block ~= nil then
            if self.block:WakeUp(ev) then
                self.sc:Add2Active(self)
            end
        end
    end

    function FTask.createTask(name,sc,action)
       local self = FTask.new() 
       local co = coroutine.create(action)
       self:init(name,sc,co)
       return self
    end
end

local FBlockStruct = FLua.Class("FBlockStruct")
do
    function FBlockStruct:_ctor()
        self.bs_type = nil
    end
    function FBlockStruct:WakeUp(type)
        return true
    end
end

local FContainer = FLua.Class("FContainer")
do
    function FContainer:_ctor()
        self.m_size = 0  --元素的数量
        self.m_data = {} --元素
    end
    function FContainer:Up(index)
        local parent_idx = self:Parent(index)
        while parent_idx > 0 do
            if self.m_data[index].timeout < self.m_data[parent_idx].timeout then
                self:swap(index,parent_idx)
                index = parent_idx
                parent_idx = self:Parent(index)
            else
                break
            end
        end
    end
    function FContainer:Down(index)
        local l = self:Left(index)
        local r = self:Right(index)
        local min = index
        
        if l <= self.m_size and self.m_data[l].timeout < self.m_data[index].timeout then
            min = l
        end

        if r <= self.m_size and self.m_data[r].timeout < self.m_data[min].timeout then
            min = r
        end

        if min ~= index then
            self:swap(index,min)
            self:Down(min)
        end
    end
    function FContainer:Parent(index)
        return index/2
    end
    function FContainer:Left(index)
        return 2*index
    end
    function FContainer:Right(index)
        return 2*index + 1
    end
    function FContainer:Change(co)
        local index = co.index
        if index == 0 then
            return
        end
        --尝试往下调整
        self:Down(index)
        --尝试往上调整
        self:Up(index)
    end
    function FContainer:Insert(co)
        if co.index ~= 0 then
            return
        end
        self.m_size = self.m_size + 1
        table.insert(self.m_data,co)
        co.index = self.m_size
        self:Up(self.m_size)
    end
    function FContainer:Min()
        if self.m_size == 0 then
            return 0
        end
        return self.m_data[1].timeout
    end
    function FContainer:PopMin()
        local co = self.m_data[1]
        self:swap(1,self.m_size)
        self.m_data[self.m_size] = nil
        self.m_size = self.m_size - 1
        self:Down(1)
        co.index = 0
        return co
    end
    function FContainer:Size()
        return self.m_size
    end
    function FContainer:swap(idx1,idx2)
        local tmp = self.m_data[idx1]
        self.m_data[idx1] = self.m_data[idx2]
        self.m_data[idx2] = tmp

        self.m_data[idx1].index = idx1
        self.m_data[idx2].index = idx2
    end
    function FContainer:Clear()
        while m_size > 0 do
            self:PopMin()
        end
        self.m_size = 0
    end
end

local FScheduler = FLua.Class("FScheduler")
do
    function FScheduler:_ctor()
        self.active_head = nil ----活动列表头
        self.active_tail = nil ----活动列表尾
        self.pending_add = {}  ----等待添加到活动列表中的FTask
        self.m_FContainer = nil
    end
    function FScheduler:init()
        self.m_FContainer = FContainer:new()
    end
    function FScheduler:GetTick()
        return os.time()
    end
    --添加到活动列表中
    function FScheduler:Add2Active(coObj)
        table.insert(self.pending_add,coObj)
    end
    --尝试唤醒uid
    function FScheduler:TryWakeup(coObj,ev)
        if coObj.block then
            coObj:Signal(ev)
        end
    end
    --强制唤醒纤程
    function FScheduler:ForceWakeup(coObj)
        if coObj.status ~= "ACTIVED" then
            coObj.sc:Add2Active(coObj)
        end
    end
    --强制唤醒阻塞在type条件上的纤程
    function FScheduler:ForceWakeup(coObj,type)
        if coObj.status ~= "ACTIVED" and coObj.block and coObj.block.bs_type == type then
            coObj.sc:Add2Active(coObj)
        end
    end
    --睡眠ms
    function FScheduler:Sleep(coObj,ms)
        if ms > 0 then
            coObj.timeout = self:GetTick() + ms
            if coObj.index == 0 then
                self.m_FContainer:Insert(coObj)
            else
                self.m_FContainer:Change(coObj)
            end
            coObj.status = "SLEEP"
        end
        coroutine.yield(coObj.co)
    end
    --暂时释放执行权
    function FScheduler:Yield(coObj)
        coObj.status = "YIELD"
        coroutine.yield(coObj.co)
    end
    --主调度循环
    function FScheduler:Schedule()
        --将pending_add中所有FTask添加到活动列表中
        for k,v in pairs(self.pending_add) do
            v.next_co = nil
            if self.active_tail ~= nil then
                self.active_tail.next_co = v
                self.active_tail = v
            else
                self.active_head = v
                self.active_tail = v
            end
        end
        
        self.pending_add = {}
        
        --运行所有可运行的FTask对象
        local cur = self.active_head
        local pre = nil
        while cur ~= nil do
            coroutine.resume(cur.co,cur)
            print("从coro中回来")
            local status = cur.status
            --当纤程处于以下状态时需要从可运行队列中移除
            if status == "DEAD" or status == "SLEEP" or status == "WAIT4EVENT" or status == "YIELD" then
                --删除首元素
                if cur == self.active_head then
                    --同时也是尾元素
                    if cur == self.active_tail then
                        self.active_head = nil
                        self.active_tail = nil
                    else
                        self.active_head = cur.next_co
                    end
                elseif cur == self.active_tail then
                    pre.next_co = nil
                    self.active_tail = pre
                else
                    pre.next_co = cur.next_co
                end

                local tmp = cur
                cur = cur.next_co
                tmp.next_co = nil
                --如果仅仅是让出处理器，需要重新投入到可运行队列中
                if status == "YIELD" then
                    self:Add2Active(tmp)
                end
            else
                pre = cur
                cur = cur.next_co
            end
        end
        --看看有没有timeout的纤程    
        local now = self:GetTick()
        while self.m_FContainer:Min() ~=0 and self.m_FContainer:Min() <= now do
            local co = self.m_FContainer:PopMin()
            if co.status == "WAIT4EVENT" or co.status == "SLEEP" then
                self:Add2Active(co)
            end
        end
    end
end

return 
{
    FTask  = FTask,
    FScheduler = FScheduler,
}
--[[test
function cofun(coObj)
    while true do
        print(coObj.name)
       coObj.sc:Sleep(coObj,1)
    end
end

function test()
  local sc = FScheduler:new()
  sc:init()

  local co1 = FCoObject:new()
  local coro1 = coroutine.create(cofun)
  co1:init("1",sc,coro1)

  local co2 = FCoObject:new()
  local coro2 = coroutine.create(cofun)
  co2:init("2",sc,coro2)

  local co3 = FCoObject:new()
  local coro3 = coroutine.create(cofun)
  co3:init("3",sc,coro3)

  local co4 = FCoObject:new()
  local coro4 = coroutine.create(cofun)
  co4:init("4",sc,coro4) 

  sc:Add2Active(co1)
  sc:Add2Active(co2)
  sc:Add2Active(co3)
  sc:Add2Active(co4) 

  while true do
    sc:Schedule()
  end
end

test()]]