
using UnityEngine;
public interface ISelectable
{
    void SwitchSelect(bool setOn);
    void SwitchSelect(bool setOn, Color color, float with);
    bool GetSelectStatus();
}
