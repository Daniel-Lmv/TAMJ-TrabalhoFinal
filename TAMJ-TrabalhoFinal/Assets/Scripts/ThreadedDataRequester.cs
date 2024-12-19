using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour
{

    static ThreadedDataRequester instance;  // Instância estática da classe
    Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();  // Fila para armazenar os dados processados nas threads

    // Método chamado ao inicializar o objeto. Ele encontra a instância do ThreadedDataRequester.
    void Awake()
    {
        instance = FindObjectOfType<ThreadedDataRequester>();
    }

    // Método público e estático para solicitar dados de forma assíncrona
    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        // Cria um delegado que inicia uma nova thread para gerar os dados
        ThreadStart threadStart = delegate {
            instance.DataThread(generateData, callback);  // Chama o método de processamento da thread
        };

        // Inicia a thread
        new Thread(threadStart).Start();
    }

    // Método que é executado na thread. Gera os dados e coloca-os na fila.
    void DataThread(Func<object> generateData, Action<object> callback)
    {
        object data = generateData();  // Gera os dados
        lock (dataQueue)
        {
            dataQueue.Enqueue(new ThreadInfo(callback, data));  // Enfileira os dados com o callback
        }
    }

    // Método chamado a cada atualização de frame. Processa a fila de dados e executa os callbacks.
    void Update()
    {
        if (dataQueue.Count > 0)
        {
            for (int i = 0; i < dataQueue.Count; i++)
            {
                ThreadInfo threadInfo = dataQueue.Dequeue();  // Obtém os dados da fila
                threadInfo.callback(threadInfo.parameter);  // Executa o callback com os dados
            }
        }
    }

    // Estrutura para armazenar informações de thread, incluindo o callback e os dados gerados
    struct ThreadInfo
    {
        public readonly Action<object> callback;  // Callback para processar os dados
        public readonly object parameter;  // Dados a serem passados para o callback

        // Construtor para inicializar os valores
        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
