;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; VACJOKE.NET Assistant Script                    ;;
;; Author: FENGberd                                ;;
;; Date: 2019/01/13                                ;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

#NoEnv
#Persistent
#SingleInstance force
#IfWinActive Counter-Strike: Global Offensive

F2::
	If State=800
	State=Off
	else
	State=800
	SetTimer SendKey, %State%
Return

SendKey:
	IfWinActive Counter-Strike: Global Offensive
	{
		Send n
	}
Return
