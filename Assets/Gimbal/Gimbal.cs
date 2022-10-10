using UnityEngine;


public interface IGimbal
{
    public GameObject GetGameObject();
}
public class Gimbal : MonoBehaviour, IGimbal
{
    public GameObject GetGameObject() => gameObject;


}