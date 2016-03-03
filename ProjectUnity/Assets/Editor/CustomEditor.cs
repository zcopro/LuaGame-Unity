#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using FLua;
using LuaInterface;

namespace FLua
{
	[CustomEditor(typeof(LuaSvrGameObject))]
	public class LuaSvrEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			LuaSvrGameObject myTarget = (LuaSvrGameObject)target;
			int bytes = LuaDLL.lua_gc(myTarget.state.L, LuaGCOptions.LUA_GCCOUNT, 0);
			EditorGUILayout.LabelField("Memory(Kb)", bytes.ToString());
			if (GUILayout.Button("Lua GC"))
			{
				LuaDLL.lua_gc(myTarget.state.L, LuaGCOptions.LUA_GCCOLLECT, 0);
			}
		}
	}
}
#endif