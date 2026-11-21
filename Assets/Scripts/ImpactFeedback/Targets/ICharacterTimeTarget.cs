// キャラクター単体の時間停止を受け取る対象
public interface ICharacterTimeTarget
{
    void RequestStop(float duration);
}