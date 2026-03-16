using UnityEngine;

public class OverlayMaximizer : MonoBehaviour
{
    static readonly int IsWindowMax = Animator.StringToHash("IsWindowMax");
    Animator _anim8R;

    void Start() {
        _anim8R = GetComponent<Animator>();
    }

    public void ToggleMaximize() {
        _anim8R.SetBool(IsWindowMax, !_anim8R.GetBool(IsWindowMax));
    }
}
