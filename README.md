# U.PC.JumpProject

Platformówka multiplayer

## Użyte technologie

- Unity

- Fishnet

- LootLocker

## Ciekawy case

### Problem
Natrafiliśmy na sytuację, w której chcieliśmy dodać "jiggly" movement do glutków (grywalnych postaci w grze). Zamierzony efekt udało nam się osiągnąć za pomocą Sprite Skinów i jointów. Problem pojawił się w momencie, gdy okazało się, że na glutka będą zakładane skiny, które muszą mieć ten sam behaviour. Skinów miało być kilkaset, a my nie mieliśmy pod ręką stażysty, więc przygotowanie sprite ręcznie nie wchodziło w grę, a unity nie wspiera automatycznego kopiowania kości. 
### Rozwiązanie
Udało mi się rozwiązać problem wykorzystując bug/feature z unity. Nie można skopiować sprite kości runtime, ale można podmienić sprite, który już ma ustawione kości na inny, przekopiować ręcznie wartości kości i nie wywoływać na końcu odświeżenia. 

### Nauka
Dzięki tej całej sytuacji dowiedziałem się, że istnieje coś takiego jak NativeArray, pracownicy unity czasami nie potrafią wytłumaczyć dlaczego coś działa i wywołałem swój pierwszy memory leak.

### Efekt końcowy
Klient zrezygnował z funkcjonalności miesiąc po implementacji.

### Wizualizacja
```public class CreateSpriteBonesFromSprite : MonoBehaviour
{
    [SerializeField] private Sprite _spriteWithBones;
    [SerializeField] private int _nativeArraySize;

    //it has to be used only once per sprite, there is no point in doing it over and over run time
    //UPDATE: unity is broken af and sometimes sprites ain't working, so we have to redo this runtime xD
    [ContextMenu("SetBones")]
    public void CreateBones()
    {
        NativeArray<BoneWeight> boneWeights = new NativeArray<BoneWeight>(_nativeArraySize, Allocator.Temp); 
        SpriteBone[] spriteBones = _spriteWithBones.GetBones();
        NativeArray<Matrix4x4> bindPoses = _spriteWithBones.GetBindPoses();
        _spriteWithBones.GetVertexAttribute<BoneWeight>(VertexAttribute.BlendWeight).CopyTo(boneWeights);
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        sprite.SetBones(spriteBones);
        sprite.SetBindPoses(bindPoses);
        sprite.SetVertexAttribute<BoneWeight>(VertexAttribute.BlendWeight, boneWeights);
        boneWeights.Dispose();

        //note to future self: DON'T FUCK WITH VERTEXATTRIBUTE.POSITION
    }
    
}```
![image](https://github.com/5Z3L4/Swampies_2021/assets/58202728/ea718153-4d71-481b-86d3-198a199aea99)
![image](https://github.com/5Z3L4/Swampies_2021/assets/58202728/6c00114b-6920-4250-a96f-873d58718d91)


https://github.com/5Z3L4/Swampies_2021/assets/58202728/87dcac5f-8474-45c9-b2a6-c721428c764b


