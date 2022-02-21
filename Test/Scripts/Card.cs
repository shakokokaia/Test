using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Networking;

public enum CARD_STATUS { IN_HAND, DRAGGING, PLACED}

public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public bool WasCardPlaced { get; private set; }
    public CARD_STATUS CardStatus { get; private set; }
    public Collider2D CardCollider { get; private set; }

    public System.Action<Card> OnDestroy;
    public System.Action<Card> OnDragStart;
    public System.Action<Card> OnDragEnd;

    [SerializeField] SpriteRenderer _backgroundShineImage;
    [SerializeField] SpriteRenderer _backgroundImage; 
    [SerializeField] SpriteRenderer _cardVisuals;
    [SerializeField] Collider2D _cardCollider;
    [SerializeField] TMP_Text Text_Mana;
    [SerializeField] TMP_Text Text_Health;
    [SerializeField] TMP_Text Text_Attack;
    [SerializeField] TMP_Text Text_Name;
    [SerializeField] TMP_Text Text_Description;

    private int _currentHealth;
    private int _currentMana;
    private int _currentAttack;

    private int _previousHealthValue;
    private int _previousManaValue;
    private int _previousAttackValue;

    private CardData _cardData;
    private List<ModifierData> _currentCardModifiers = new List<ModifierData>();

    private Vector3 _dragOffset;

    private void Awake()
    {
        CardCollider = GetComponent<Collider2D>();
    }

    public void SetData(CardData cardData)
    {
        CardStatus = CARD_STATUS.IN_HAND;
        _cardData = cardData;
        _currentHealth = _cardData.DefaultHealth;
        _currentAttack = _cardData.DefaultAttack;
        _currentMana = _cardData.DefaultManaCost;
        Text_Name.text = _cardData.CardName;
        Text_Description.text = _cardData.CardDescription;
        Text_Attack.text = _cardData.DefaultAttack.ToString();
        Text_Health.text = _cardData.DefaultHealth.ToString();
        Text_Mana.text = _cardData.DefaultManaCost.ToString();

        StartCoroutine(SetImage());
    }

    public void UpdateCardUI()
    {
        DoTextAnimation(Text_Health, _previousHealthValue, _currentHealth);
        DoTextAnimation(Text_Mana, _previousManaValue, _currentMana);
        DoTextAnimation(Text_Attack, _previousAttackValue, _currentAttack);

        Text_Name.text = _cardData.CardName;
        Text_Description.text = _cardData.CardDescription;
    }

    private void DoTextAnimation(TMP_Text textToChange, int previousValue, int currentValue)
    {
        DOTween.To(() => previousValue, x => previousValue = x, currentValue, 0.5f).OnUpdate(() =>
        {
            textToChange.text = previousValue.ToString();
        });
    }

    public void SetHandLocation(Transform location)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, location.position.z);
        transform.DOMove(location.position, 0.5f);
        transform.DORotate(location.rotation.eulerAngles, 0.5f);
    }

    public void AddCardModifier(ModifierData modifierData)
    {
        _currentCardModifiers.Add(modifierData);
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        for (int i = 0; i < _currentCardModifiers.Count; i++)
        {
            ModifierData modifier = _currentCardModifiers[i];

            if(modifier.Type == MODIFIER_TYPE.HEALTH)
            {
                _previousHealthValue = _currentHealth;

                if (modifier.IsOverride)
                    _currentHealth = modifier.Value;
                else
                    _currentHealth += modifier.Value;
            }
            else if (modifier.Type == MODIFIER_TYPE.MANA)
            {
                _previousManaValue = _currentMana;

                if (modifier.IsOverride)
                    _currentMana = modifier.Value;
                else
                    _currentMana += modifier.Value;
            }
            else if (modifier.Type == MODIFIER_TYPE.ATTACK)
            {
                _previousAttackValue = _currentAttack;

                if (modifier.IsOverride)
                    _currentAttack = modifier.Value;
                else
                    _currentAttack += modifier.Value;
            }
        }

        UpdateCardUI();

        if(_currentHealth <= 0)
        {
            DestroyCard();
        }
    }

    private void DestroyCard()
    {
        OnDestroy?.Invoke(this);
        Destroy(this.gameObject, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Camera.main.ScreenToWorldPoint(eventData.position) + _dragOffset;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragOffset = transform.position - Camera.main.ScreenToWorldPoint(eventData.position) - Vector3.forward*8f;

        CardStatus = CARD_STATUS.DRAGGING;
        _backgroundShineImage.enabled = true;
        transform.DORotate(Vector3.zero, 0.2f);

        OnDragStart?.Invoke(this);
    }

    public void SetCardPlaced()
    {
        WasCardPlaced = true;
        CardStatus = CARD_STATUS.PLACED;
    }

    public void SetCardReturned()
    {
        CardStatus = CARD_STATUS.IN_HAND;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _backgroundShineImage.enabled = false;
        OnDragEnd?.Invoke(this);
    }

    IEnumerator SetImage()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture($"{_cardData.ImageURL}");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetTexture("_MainTex", texture);
            _cardVisuals.SetPropertyBlock(block);
        }
    }
}