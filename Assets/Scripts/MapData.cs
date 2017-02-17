using UnityEngine;
using System.Collections;

using System.Text;

public class MapData  {
    public Vector3 middle = Vector3.zero;
    public bool isActive = true;
    public MapData(Vector3 point) {
        middle = point;
    }

    override public string ToString() {
        StringBuilder str = new StringBuilder();
        str.Append(middle.ToString()+","+isActive.ToString());
        return str.ToString();
    }
}
