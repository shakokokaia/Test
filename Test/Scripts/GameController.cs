using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] List<CardData> _p1CardPool = new List<CardData>();
    [SerializeField] List<Transform> _p1CardLocations = new List<Transform>();
    [SerializeField] List<Transform> _p1CardBoardLocations = new List<Transform>();
    [SerializeField] GameObject _cardPrefab;
    [SerializeField] Collider2D _boardCollider;
    [SerializeField] Transform _p1CardSpawnLocation;
    [SerializeField] List<ModifierData> _demoModifiers = new List<ModifierData>();

    private List<Card> _p1Cards = new List<Card>();
    private Collider2D[] _cardDragColliderResults = new Collider2D[5];

    private int _currentDemoChangeIndex;

    private void Start()
    {
        int randCardCount = Random.Range(4, 7);
        StartCoroutine(PickCards(randCardCount));
    }

    IEnumerator PickCards(int count)
    {
        int pickedCount = 0;

        while (pickedCount < count && _p1CardPool.Count != 0)
        {
            Card card = Instantiate(_cardPrefab, _p1CardSpawnLocation.position, Quaternion.identity).GetComponent<Card>();
            card.SetData(_p1CardPool[0]);
            card.SetHandLocation(_p1CardLocations[_p1CardLocations.Count / 2]);

            card.OnDragStart = OnCardDragStart;
            card.OnDragEnd = OnCardDragEnd;
            card.OnDestroy = OnCardDestroy;

            _p1CardPool.RemoveAt(0);
            _p1Cards.Add(card);


            pickedCount++;

            yield return new WaitForSeconds(0.5f);

            RepositionCards();

            yield return new WaitForSeconds(0.6f);
        }
    }

    public void RandomizeCardStats()
    {
        if (_currentDemoChangeIndex >= _p1Cards.Count) _currentDemoChangeIndex = 0;

        for (int i = 0; i < _demoModifiers.Count; i++)
        {
            ModifierData modifier = _demoModifiers[i];
            modifier.Value = Random.Range(-2, 9);

            if (_p1Cards.Count == 0) return;
            _p1Cards[_currentDemoChangeIndex].AddCardModifier(modifier);
        }

        if (_currentDemoChangeIndex + 1 < _p1Cards.Count) _currentDemoChangeIndex++;
        else _currentDemoChangeIndex = 0;
    }

    private void OnCardDragStart(Card draggedCard)
    {
        RepositionCards();
    }

    private void OnCardDestroy(Card destroyedCard)
    {
        _p1Cards.Remove(destroyedCard);
        RepositionCards();
    }

    private void OnCardDragEnd(Card draggedCard)
    {
        int Count = draggedCard.CardCollider.OverlapCollider(new ContactFilter2D ().NoFilter(), _cardDragColliderResults);
        bool isTouching = false;

        if(Count > 0)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_cardDragColliderResults[i].CompareTag("Board"))
                {
                    isTouching = true;
                }
            }
        }

        if ((isTouching && _p1Cards.FindAll(x => x.CardStatus == CARD_STATUS.DRAGGING).Count <= _p1CardBoardLocations.Count) || draggedCard.WasCardPlaced)
        {
            draggedCard.SetCardPlaced();
            RepositionBoard();
        }
        else
        {
            draggedCard.SetCardReturned();
        }

        RepositionCards();
    }

    private void RepositionBoard()
    {
        int freeBoardSpace = _p1CardBoardLocations.Count - _p1Cards.FindAll(x => x.CardStatus == CARD_STATUS.PLACED).Count;
        int startIndex = Mathf.FloorToInt((float)freeBoardSpace / 2f);
        int currentCardIndex = startIndex;

        for (int i = 0; i < _p1Cards.Count; i++)
        {
            if (_p1Cards[i].CardStatus == CARD_STATUS.PLACED)
            {
                _p1Cards[i].SetHandLocation(_p1CardBoardLocations[currentCardIndex]);
                currentCardIndex++;
            }
        }
    }

    private void RepositionCards()
    {
        int freeCardSpace = 
            _p1CardLocations.Count 
            - _p1Cards.Count 
            + _p1Cards.FindAll(x => (x.CardStatus == CARD_STATUS.DRAGGING || x.CardStatus == CARD_STATUS.PLACED)).Count;

        int startIndex = Mathf.FloorToInt((float)freeCardSpace / 2f);
        int currentCardIndex = startIndex;

        for (int i = 0; i < _p1Cards.Count; i++)
        {
            if(_p1Cards[i].CardStatus == CARD_STATUS.IN_HAND)
            {
                _p1Cards[i].SetHandLocation(_p1CardLocations[currentCardIndex]);
                currentCardIndex++;
            }
        }
    }
}
