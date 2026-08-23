using Cysharp.Threading.Tasks;
using UnityEngine;

public class WeaponSetting
{
    public async UniTask<IWeapon> Execute(WeaponType type, GameObject _parent) => await WeaponTypeToObject(type, _parent);

    private async UniTask<IWeapon> WeaponTypeToObject(WeaponType type, GameObject _parent)
    {
        var factory = Locator<Factory>.Get();
        var resourceManager = Locator<ResourceManager>.Get();
        IWeapon weapon = null;

        Quaternion weaponRotation = Quaternion.identity;

        switch (type)
        {
            case WeaponType.Knife:
                var knifeOrigin = await resourceManager.Get<GameObject>("Knife");
                var knifeComponent = knifeOrigin.GetComponent<Knife>();
                var knife = factory.Create<Knife>(knifeComponent, Vector3.zero, weaponRotation, parent: _parent.transform);
                weapon = knife;
                break;
            case WeaponType.Rifle:
                // 라이플 초기인 글록
                var glockOrigin = await resourceManager.Get<GameObject>("Glock");
                var rifleComponent = glockOrigin.GetComponent<Rifle>();
                var glock = factory.Create<Rifle>(rifleComponent, Vector3.zero, weaponRotation, parent: _parent.transform);
                weapon = glock;
                break;
            case WeaponType.Sniper:
                // 스나이퍼 초기인 R1895
                var R1895Origin = await resourceManager.Get<GameObject>("R1895");
                var sniperComponent = R1895Origin.GetComponent<Sniper>();
                var r1895 = factory.Create<Sniper>(sniperComponent, Vector3.zero, weaponRotation, parent: _parent.transform);
                weapon = r1895;
                break;
        }

        return weapon;
    }


}
