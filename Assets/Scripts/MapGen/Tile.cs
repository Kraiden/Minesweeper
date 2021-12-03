using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AndroidNativeCore;
using TMPro;

public class Tile{
        public static System.Action<Tile> OnBombHit;
        public static System.Action<bool> OnFlagChange;
        public static System.Action OnCheckWin;

        public static System.Action<ParticleSystem,Vector3> playEffect;

        public static ParticleSystem popEffect;
        public static ParticleSystem flagEffect;
        public static ParticleSystem revealEffect;

        public static Color overTileStartColor;
        public static Color flagColor;

        public static Color[] countColours;

        public Coord coord;
        public Transform overTile;
        public Transform underTile;
        public Transform bomb;

        public bool isRevealed {private set; get;} = false;
        public bool isBomb {private set; get;} = false;
        public bool isFlagged {private set; get;} = false;

        public int adjBombs;

        public Tile(Coord _coord, Transform _overTile, Transform _underTile, Transform _bomb){
            this.coord = _coord;
            this.overTile = _overTile;
            this.underTile = _underTile;
            this.bomb = _bomb;
        }

        public void Reset(){
            overTile.gameObject.SetActive(true);
            overTile.GetComponent<Renderer>().material.color = overTileStartColor;

            isRevealed = false;
            isFlagged = false;
            isBomb = false;

            adjBombs = 0;
        }

        public void SetBomb(bool _isBomb, int adjCount = 0){
            isBomb = _isBomb;

            if(isBomb){
                underTile.gameObject.SetActive(false);
                bomb.gameObject.SetActive(true);
            } else {
                bomb.gameObject.SetActive(false);
                underTile.gameObject.SetActive(true);

                TextMeshPro tmp = underTile.gameObject.GetComponentInChildren<TextMeshPro>();
                adjBombs = adjCount;
                if(adjCount > 0){
                    tmp.text = adjCount.ToString();
                    tmp.color = countColours[adjCount -1];
                } else {
                    tmp.text = "";
                }
            }
        }

        public bool Reveal(){
            bool revealed = false;

            if(!isFlagged && !isRevealed){
                isRevealed = true;
                revealed = true;

                AudioManager.instance.PlaySound2d("reveal",.5f);

                overTile.gameObject.SetActive(false);

                Vector3 position = overTile.transform.position;
                playEffect(revealEffect, position);
                OnCheckWin();
            }

            if(isBomb && isRevealed){
                if(OnBombHit != null){
                    OnBombHit(this);
                    if(PlayerPrefs.GetInt("settings-vib", 1) == 1){
                        Vibrator.Vibrate(200);
                    }
                }
            }

            return revealed;
        }

        public void Pop(float vol){
            if(!isRevealed){
                isRevealed = true;
                AudioManager.instance.PlaySound2d("pop", vol);

                overTile.gameObject.SetActive(false);

                Vector3 position = underTile.transform.position;
                position += Vector3.back * 6f;

                playEffect(popEffect, position);
            }
        }

        public void Flag(){
            if(!isRevealed){
                isFlagged = !isFlagged;
                if(isFlagged){
                    overTile.GetComponent<Renderer>().material.color = flagColor;
                } else {
                    overTile.GetComponent<Renderer>().material.color = overTileStartColor;
                }

                if(OnFlagChange != null){
                    OnFlagChange(isFlagged);
                }

                AudioManager.instance.PlaySound2d("flag");

                Vector3 position = overTile.transform.position;
                playEffect(flagEffect, position);

                if(PlayerPrefs.GetInt("settings-vib", 1) == 1){
                    Vibrator.Vibrate(100);
                }
            }
        }
    }