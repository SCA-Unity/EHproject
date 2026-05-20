using UnityEngine;
using TwoBitMachines.FlareEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 10f;          // 맞으면 빠질 체력 (양수로 설정, 내부에서 -로 적용)
    [SerializeField] private float damageForce = 10f;     // 넉백 힘 (다른 시스템과 이름 맞춤)

    [Header("Collision Type")]
    [SerializeField] private bool useTrigger = true;      // 트리거로 쓸지 여부

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTrigger)
            return;

        DealDamage(other.transform, other.bounds.ClosestPoint(transform.position));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useTrigger)
            return;

        if (collision.contactCount > 0)
        {
            DealDamage(collision.transform, collision.contacts[0].point);
        }
        else
        {
            DealDamage(collision.transform, collision.transform.position);
        }
    }

    private void DealDamage(Transform target, Vector2 hitPoint)
    {
        if (target == null)
            return;

        // Health 컴포넌트가 붙어 있고, Health 딕셔너리에 등록된 캐릭터만 데미지 적용
        if (!Health.IsDamageable(target))
            return;

        // 기존 FlareEngine 예제들과 동일한 패턴:
        // direction * damageForce 를 넘겨서 넉백/피격 이펙트가 같은 방식으로 동작
        Vector2 direction = ((Vector2)target.position - hitPoint).normalized;
        if (direction == Vector2.zero)
        {
            direction = Vector2.up;
        }

        Health.IncrementHealth(transform, target, -damage, direction * damageForce);
    }
}

