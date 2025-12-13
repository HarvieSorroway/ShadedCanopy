using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils.SCDevTools.NodeTreeManager
{
    //public class SCDevToolsNodeTreeBranch : Attribute
    //{
    //    public string branchPatch;
    //    public SCDevToolsNodeTreeBranch(string branchPath)
    //    {
    //        branchPatch = branchPath;
    //    }
    //}

    //public class SCDevToolsNodeTreeLeafName : Attribute
    //{
    //    public string leafName;
    //    public SCDevToolsNodeTreeLeafName(string leafName)
    //    {
    //        this.leafName = leafName;
    //    }
    //}

    public class SCDevToolsInspectType : Attribute
    {
        public string branchPatch;
        public string leafName;
        public SCDevToolsInspectType(string branchPath, string leafName)
        {
            this.branchPatch = branchPath;
            this.leafName = leafName;
        }
    }

    public class SCDevToolsInspectValue : Attribute
    {
        public SCDevToolsInspectValue() { }
    }

    public abstract class SCDevToolsSpecialFieldType : Attribute
    {
        public SCDevToolsSpecialFieldType() { }
    }

    public sealed class SCDevToolsRangeField : SCDevToolsSpecialFieldType
    {
        public float min, max, defaultVal;
        public SCDevToolsRangeField(float min, float max, float defaultVal)
        {
            this.min = min;
            this.max = max;
            this.defaultVal = defaultVal;
        }
    }

    public sealed class SCDevToolsListBoxStringField: SCDevToolsSpecialFieldType
    {
        public string[] options;
        public int defaultIndex;
        public SCDevToolsListBoxStringField(string[] options, string defaultVal)
        {
            this.options = options;
            this.defaultIndex = options.IndexOf(defaultVal);
        }
    }
}
