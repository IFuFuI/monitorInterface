Public Class Palabras_Clave
    Public ReadOnly strTxn_I_JOURNAL As String = "INICIO JOURNAL"
    Public ReadOnly strTxn_I As String = "INICIO TRANSACCION"
    Public ReadOnly strTxn_F As String = "FIN DE TRANSACCION"
    Public ReadOnly strSolicitudTxn As String = "SOLICITUD DE TRANS"

    Public T_ST As New Txn_SinTarj
    Public Val_NIP As New Validacion_NIP
    Public CC As New Consulta_Cheques
    Public CA As New Consulta_Ahorro
    Public CCredito As New Consulta_Credito
    Public Retiro As New Retiro
    Public Retiro_FC As New Retiro_FastCash
    Public Retiro_OB As New Retiro_Otros_Bancos
    Public DM As New Dinero_Movil
    Public Cam_NIP As New Cambio_NIP
    Public Impr_Recibo_Ret As New ImpresionRecibo_Retiro
    Public C_TA_Telcel As New Compra_TA_Telcel
    Public C_TA_Nextel As New Compra_TA_Nextel
    Public C_TA_Movi As New Compra_TA_Movi
    Public Pago_Axtel As New PagoAxtel
    Public Traspaso As New Traspaso
    Public Pago_TC_BBVA As New Pago_TC_Bancomer
    Public Dotacion As New Dotacion

End Class

Public Class Dotacion

    Public ReadOnly strInicioFisica As String = "ADD CASH"
    Public ReadOnly strInicioLogica As String = "A D I C I O N"
    Public ReadOnly strFinGoodFisica As String = "CAJERO EN SERVICIO"
    Public ReadOnly strFinGoodLogica As String = "FIN DE TRANSACCION"
End Class
Public Class Validacion_NIP
    Public ReadOnly strInicio As String = "SOLICITUD DE TRANS: AABB  AA"
    Public ReadOnly strFinGood As String = "ESTADO: A12 FUNCION: U"
End Class
Public Class Txn_SinTarj
    Public ReadOnly strInicio As String = "INICIO RETIRO SIN TARJETA"
    Public ReadOnly strFinGood As String = "RETIRO SIN TARJETA DISPENSO OK ENVIO MENSAJE"
    'Public Const strFinBad_Clave As String = "FIN NO OK RETIRO SIN TARJETA"

End Class

Public Class Consulta_Cheques
    Public strInicio As String = "CONSULTA DE CHEQUES"
    'Public strInicio As String = "SOLICITUD DE TRANS: CA"
    Public strFinGood As String = "ESTADO: 076 FUNCION: 5"
End Class

Public Class Consulta_Credito
    Public strInicio As String = "CONSULTA DE CHEQUES"
    'Public strInicio As String = "SOLICITUD DE TRANS: CA"
    Public strFinGood As String = "ESTADO: 076 FUNCION: 3"
End Class


Public Class Consulta_Ahorro
    Public strInicio As String = "CONSULTA DE AHORRO"
    Public strInicio_1 As String = "SOLICITUD DE TRANS: CB"
    Public strInicio_2 As String = "SOLICITUD DE TRANS: CC"
    Public strFinGood As String = "ESTADO: 076 FUNCION: 5"
End Class

Public Class Retiro
    Public strInicio As String = "SOLICITUD DE TRANS: AA     D"
    Public strFinGood As String = "BILLETES PRESENTADOS"
End Class

Public Class Retiro_FastCash
    Public strInicio_1 As String = "SOLICITUD DE TRANS: AB     D"
    Public strInicio_2 As String = "SOLICITUD DE TRANS: CC"
    Public strFinGood As String = "BILLETES PRESENTADOS"
End Class
Public Class Retiro_Otros_Bancos
    Public strInicio As String = "SOLICITUD DE TRANS: AB"
    Public strFinGood As String = "BILLETES PRESENTADOS"
End Class

Public Class Dinero_Movil
    Public strInicio As String = "SOLICITUD DE TRANS: AABB  DF"
    Public strFinGood As String = "RETIRO SIN TARJETA DISPENSO OK ENVIO MENSAJE"
End Class


Public Class Cambio_NIP
    Public strInicio As String = "SOLICITUD DE TRANS: BC"
    Public strFinGood As String = "ESTADO: 121 FUNCION: 5"
End Class

Public Class ImpresionRecibo_Retiro
    Public strInicio_1 As String = "SOLICITUD DE TRANS: AA  CC"
    Public strInicio_2 As String = "SOLICITUD DE TRANS: AA  CC D"
    Public strFinGood As String = "ESTADO: 429 FUNCION: 5"
End Class

Public Class Compra_TA_Telcel
    Public strInicio As String = "SOLICITUD DE TRANS:        C"
    Public strFinGood As String = "ESTADO: A72 FUNCION: 3"
End Class

Public Class Compra_TA_Movi
    Public strInicio As String = "SOLICITUD DE TRANS:        B"
    Public strFinGood As String = "ESTADO: A72 FUNCION: 3"
End Class

Public Class Compra_TA_Nextel
    Public strInicio As String = "SOLICITUD DE TRANS:    D   D"
    Public strFinGood As String = "ESTADO: A72 FUNCION: 3"
End Class

Public Class PagoAxtel
    Public strInicio As String = "SOLICITUD DE TRANS: A D"
    Public strFinGood As String = "ESTADO: 243 FUNCION: 3"
End Class

Public Class Traspaso
    Public strInicio As String = "SOLICITUD DE TRANS: DCB"
    Public strFinGood As String = "FUNCION: 5"
End Class

Public Class Pago_TC_Bancomer
    Public strInicio As String = "SOLICITUD DE TRANS: DBCD"
    Public strFinGood As String = "ESTADO: 630 FUNCION: 3"
End Class
