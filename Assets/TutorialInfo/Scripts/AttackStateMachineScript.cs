using UnityEngine;

public class AttackStateMachineScript : StateMachineBehaviour
{

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        // explicação do componentes:
        // O método OnStateExit é chamado quando o estado de ataque é encerrado.
        // Dentro deste método, obtemos uma referência ao script Player_moviment anexado ao mesmo GameObject que o Animator.
        // Em seguida, chamamos o método OnAttackAnimationEnd do script Player_moviment para notificar que a animação de ataque terminou.
        // animator é o parâmetro que representa o componente Animator ao qual este StateMachineBehaviour está anexado.
        // stateInfo contém informações sobre o estado atual do Animator.
        // layerIndex indica a camada do Animator na qual o estado está sendo executado.
        // para acessar o script Player_moviment, usamos animator.GetComponent<Player_moviment>().
        // Isso nos permite chamar o método OnAttackAnimationEnd definido no script Player_moviment.
        // si quiser adicionar mais lógica quando a animação de ataque termina, você pode fazê-lo dentro deste método.
        Debug.Log("Saindo do estado de ataque");
        // com animador pegamos o GameObject que possui o Animator e acessamos o script Player_moviment anexado a ele
        PlayerStateMachine playerMovement = animator.GetComponent<PlayerStateMachine>();
        // Chama o método OnAttackAnimationEnd do script Player_moviment
        
        playerMovement.OnAttackAnimationEnd();
    }

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
    }
}
