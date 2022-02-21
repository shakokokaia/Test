using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Test/Cards/New Card")]
public class CardData : ScriptableObject
{
    [SerializeField] int _defaultManaCost;
    [SerializeField] int _defaultAttack;
    [SerializeField] int _defaultHealth;
    [SerializeField] string _cardName;
    [SerializeField] string _cardDescription;
    [SerializeField] string _imageURL;

    public int DefaultManaCost => _defaultManaCost;
    public int DefaultAttack => _defaultAttack;
    public int DefaultHealth => _defaultHealth;
    public string CardName => _cardName;
    public string CardDescription => _cardDescription;
    public string ImageURL => _imageURL;

}
