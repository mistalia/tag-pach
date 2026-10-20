// コントローラーの登録方式
public enum ControllerBindingMode
{
    // 1チームにつきOperation・Placementの2台を使用
    FourControllers,

    // 1台でOperation・Placementの両方を担当
    TwoControllersTest
}