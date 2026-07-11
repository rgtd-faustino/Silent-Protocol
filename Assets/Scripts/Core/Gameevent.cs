using System;

// #my_code - Sistema de eventos desacoplados (publisher/subscriber) que substitui polling de booleans.


// Em vez de um script chamar diretamente o método de outro, dispara-se aqui um evento global e
// quem precisar de reagir subscreve-se (+=) no OnEnable/Awake/Start e desinscreve-se (-=) no OnDisable/OnDestroy
// assim o TimeManager, por exemplo, não precisa de saber que o TaskManager ou o NPCManager existem, só dispara o evento e esquece
public static class GameEvent {
    // CICLO DO DIA

    // DISPARADO POR: GameManager.HandleDayEnd(), sempre que um dia de trabalho termina (reage ao OnDayEnded) e currentDay ainda não chegou ao TotalDays (5)
    // no dia 5 este evento não dispara, em vez disso corre TriggerReportEnding() diretamente
    // SUBSCRITO POR:
    // - CameraSystem.OnDayChangedHandler (zera cumulativeWatchSeconds para o novo dia)
    // - CardCredentialPickup.HandleDayChanged (ativa o cartão se day >= diaParaAparecer)
    // - IntelPickup.HandleDayChanged (ativa a intel se day >= diaParaAparecer)
    // O QUE MUDA NO JOGO: os cartões de acesso e a intel agendados para esse dia aparecem no mapa e o contador de abuso de câmaras volta a zero
    public static event Action<int> OnDayChanged;



    // DISPARADO POR: TimeManager.FireDayEvents(), quando currentMinutes ultrapassa as 09:00 (WorkStartMinute) uma vez por dia, guardado pela flag firedWorkStart
    // SUBSCRITO POR: TaskManager.ActivateTasks(), destrói a UI das tasks do dia anterior, carrega o DaySchedule do dia atual e cria as TaskEntry novas
    // O QUE MUDA NO JOGO: a lista de tarefas do dia fica pronta e as tasks passam a poder aparecer (TrySpawnTask) à hora marcada no schedule.
    public static event Action OnWorkHoursStarted;



    // DISPARADO POR: TimeManager.FireDayEvents(), quando currentMinutes ultrapassa as 22:00 (NightStartMinute), uma vez por dia, guardado pela flag firedNight
    // SUBSCRITO POR: FlashlightController.OnNightStarted (desliga a lanterna e recarrega a bateria a 100%)
    // O QUE MUDA NO JOGO: a bateria da lanterna reinicia cheia para a noite
    public static event Action OnNightStarted;



    // DISPARADO POR: TimeManager, em dois sítios:
    // 1) Update(), quando currentMinutes ultrapassa as 24:00 (DayEndMinute), chegou a meia noite sem o jogador ter ido dormir (guarda: firedDayEnd)
    // 2) Sleep(wakeUpHours), sempre que o jogador dorme, seja a que horas for
    // SUBSCRITO POR: GameManager.HandleDayEnd(), guarda o progresso, se ainda não for o último dia incrementa currentDay e dispara OnDayChanged
    // e se já for o último dia, dispara o final do relatório em vez disso
    // O QUE MUDA NO JOGO: o dia civil avança (ou decide-se o final, no dia 5) e o progresso fica guardado em disco.
    public static event Action OnDayEnded;





    // ESTADO DO JOGO

    // DISPARADO POR: SuspicionManager.CheckStateChange(), só quando o ratio de suspeita chega a 100% (o estado passa a Expulsion), dispara sempre logo a seguir a SuspicionStateChanged(Expulsion)
    // SUBSCRITO POR: GameManager.HandleGameOver(), toca o som de "apanhado" e dispara o final 3 (apanhado pela suspeita)
    // O QUE MUDA NO JOGO: game over —> o jogador foi apanhado e o ecrã de final aparece
    public static event Action OnGameOver;


    // DISPARADO POR: TimeManager.Update(), a cada frame em que GetSleepStage() chega a 3 (estágio severo de fadiga acumulada), guardado pela flag firedExhaustion
    // firedExhaustion só volta a false no Sleep() (o jogador dormiu) ou no ResetForNewGame(), ao contrário das outras flags do dia, não é reposta se passar da meia-noite (ResetDayFlags() não a inclui)
    // por isso passar para o dia seguinte sem dormir não faz o evento voltar a disparar
    // SUBSCRITO POR: GameManager.HandleExhaustion(), dispara o final 4 (exaustão) via FireEnding(4)
    // O QUE MUDA NO JOGO: game over —> o jogador nunca mais dormiu e o ecrã de final aparece
    public static event Action OnPlayerExhausted;




    // DISPARADO POR: GameManager.FireEnding(ending), chamado a partir de três sítios:
    // - TriggerReportEnding() -> ending 1 (bom) ou 2 (mau), corre quando o jogador carrega em "Enviar Relatório" no email ou automaticamente no fim do dia 5
    // - HandleGameOver() -> ending 3, quando a suspeita chega a Expulsion
    // - HandleExhaustion() -> ending 4, quando o sono acumulado chega ao estágio severo
    // FireEnding() só deixa passar a primeira chamada (guarda endingTriggered), por isso só um final acontece por partida mesmo que duas condições se cumpram ao mesmo tempo
    // SUBSCRITO POR: EndingUI.HandleEnding(ending), mostra o ecrã de final com título, descrição e todas as estatísticas da run (dia, suspeita, intel, câmaras, etc.)
    // O QUE MUDA NO JOGO: aparece o ecrã de fim de jogo correspondente
    public static event Action<int> OnEndingReached;




    // DISPARADO POR: GameManager.NotifyDayStarted(), chamado a partir de três sítios:
    // - TimeManager.FireDayEvents(), às 08:00 (DayStartMinute), uma vez por dia (guarda: firedDayStart)
    // - TimeManager.Sleep(), logo a seguir a acordar, seja a que horas for
    // - GameManager.Start() -> ShowTitleNextFrame(), uma única vez no arranque do jogo, para mostrar "Dia 1"
    // NotifyDayStarted() não dispara nada se o jogo já tiver terminado (endingTriggered)
    // SUBSCRITO POR: DayTitleUI.HandleDayStarted(day), mostra o cartão "Dia X" a meio do ecrã
    // O QUE MUDA NO JOGO: aparece o título "Dia X" a meio do ecrã
    public static event Action<int> OnDayStarted;






    // REUNIÃO

    // DISPARADO POR: NPCManager.TriggerMeeting(), chamado por TimeManager.FireDayEvents() às 17:30 (MeetingMinute), uma vez por dia (guarda: firedMeeting)
    // O TriggerMeeting() também força os NPCs para as rotas de reunião, isto corre sempre
    // SUBSCRITO POR: MeetingEavesdropScript.OnMeetingStarted(), liga meetingActive a true e zera capturedCount, o que ativa a deteção de proximidade e o minijogo de escutar a reunião pela porta
    // O QUE MUDA NO JOGO: os NPCs vão para a sala de reuniões e a zona junto à porta passa a reagir ao jogador (sobe suspeita se ele lá ficar especado e permite iniciar o minijogo)
    public static event Action OnMeetingStarted;





    // SUSPEITA

    // DISPARADO POR: SuspicionManager.CheckStateChange(), sempre que o ratio de suspeita atravessa um dos limiares (33% / 66% / 100%) e o estado (None/Attention/Investigation/
    // Expulsion) muda mesmo, não dispara a cada frame, só na transição
    // SUBSCRITO POR: SuspicionHUD.HandleStateChanged(state), muda a cor e o texto do HUD (olho + anel radial) e liga/desliga a animação de pulsar
    // OnSuspicionStateChanged(state), propaga o novo estado a cada NPCScript ativo via OnGlobalSuspicionChanged()
    // O QUE MUDA NO JOGO: o HUD de suspeita muda de cor e texto e os NPCs ajustam o comportamento ao novo nível de alerta
    public static event Action<SuspicionManager.SuspicionState> OnSuspicionStateChanged;





    // EMAIL CRÍTICO

    // DISPARADO POR: PCEmailManager.Update(), para cada email crítico cujo temporizador de auto-delete (autoDeleteTimers, criado quando o email é entregue com
    // autoDeleteGameMinutes > 0) chega a zero, o email já foi removido da inbox e do lixo antes deste evento disparar
    // SUBSCRITO POR: EmailUI.OnCriticalEmailExpired(emailID), se esse email era o que estava aberto no painel de detalhe, fecha o painel e depois atualiza a lista
    // O QUE MUDA NO JOGO: o email crítico desaparece sozinho da inbox se o jogador não for a tempo de o ler/decidir e e se o tinha aberto o painel fecha-se
    public static event Action<string> OnCriticalEmailExpired;




    // Invocadores
    // cada método aqui é só o "botão" que os outros scripts carregam para disparar o evento com o mesmo nome lá em cima
    // o "?." faz com que nunca dá erro mesmo que, por acaso, ninguém esteja subscrito

    public static void DayChanged(int day) => OnDayChanged?.Invoke(day);
    public static void DayStarted(int day) => OnDayStarted?.Invoke(day);
    public static void EndingReached(int ending) => OnEndingReached?.Invoke(ending);
    public static void WorkHoursStarted() => OnWorkHoursStarted?.Invoke();
    public static void NightStarted() => OnNightStarted?.Invoke();
    public static void DayEnded() => OnDayEnded?.Invoke();
    public static void GameOver() => OnGameOver?.Invoke();
    public static void SuspicionStateChanged(SuspicionManager.SuspicionState s) => OnSuspicionStateChanged?.Invoke(s);
    public static void MeetingStarted() => OnMeetingStarted?.Invoke();
    public static void CriticalEmailExpired(string id) => OnCriticalEmailExpired?.Invoke(id);
    public static void PlayerExhausted() => OnPlayerExhausted?.Invoke();

}