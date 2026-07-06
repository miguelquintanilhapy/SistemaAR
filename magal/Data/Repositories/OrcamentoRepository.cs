using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using magal.Data;
using magal.Models;

namespace magal.Data.Repositories
{
    public class OrcamentoRepository
    {
        /// <summary>
        /// Salva o orçamento principal e todos os seus itens vinculados de uma só vez usando Transação.
        /// </summary>
        public async Task<int> SalvarCompletoAsync(Orcamento orcamento, List<ItemOrcamento> itens)
        {
            using (var conn = (MySqlConnection)DbConnectionFactory.CreateConnection())
            {
                await conn.OpenAsync();

                // Iniciamos uma transação para garantir que se um item falhar, nada seja salvo incorretamente
                using (var transacao = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Inserir o orçamento principal
                        string sqlOrcamento = @"
                            INSERT INTO orcamento 
                            (cliente_nome, cliente_telefone, veiculo_marca, veiculo_modelo, 
                             veiculo_placa, veiculo_cor, veiculo_ano, garantia_dias, 
                             observacoes, valor_total)
                            VALUES 
                            (@nome, @tel, @marca, @modelo, @placa, @cor, @ano, @garantia, @obs, @total);
                            SELECT LAST_INSERT_ID();"; // Pega o ID gerado automaticamente

                        int orcamentoId = 0;

                        using (var cmd = new MySqlCommand(sqlOrcamento, conn, transacao))
                        {
                            cmd.Parameters.AddWithValue("@nome", orcamento.ClienteNome);
                            cmd.Parameters.AddWithValue("@tel", (object)orcamento.ClienteTelefone ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@marca", (object)orcamento.VeiculoMarca ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@modelo", (object)orcamento.VeiculoModelo ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@placa", (object)orcamento.VeiculoPlaca ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@cor", (object)orcamento.VeiculoCor ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ano", orcamento.VeiculoAno);
                            cmd.Parameters.AddWithValue("@garantia", orcamento.GarantiaDias);
                            cmd.Parameters.AddWithValue("@obs", (object)orcamento.Observacoes ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@total", orcamento.ValorTotal);

                            orcamentoId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        }

                        // 2. Inserir os itens vinculados a esse orçamento
                        string sqlItem = @"
                            INSERT INTO item_orcamento (orcamento_id, descricao, valor_unitario) 
                            VALUES (@orcamentoId, @descricao, @valorUnitario);";

                        foreach (var item in itens)
                        {
                            using (var cmdItem = new MySqlCommand(sqlItem, conn, transacao))
                            {
                                cmdItem.Parameters.AddWithValue("@orcamentoId", orcamentoId);
                                cmdItem.Parameters.AddWithValue("@descricao", item.Descricao);
                                cmdItem.Parameters.AddWithValue("@valorUnitario", item.ValorUnitario);

                                await cmdItem.ExecuteNonQueryAsync();
                            }
                        }

                        // Confirma todas as inserções no banco
                        await transacao.CommitAsync();
                        return orcamentoId;
                    }
                    catch (Exception)
                    {
                        // Desfaz tudo se der algum erro
                        await transacao.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Busca todos os orçamentos salvos para listar na tabela de histórico da tela principal.
        /// </summary>
        public async Task<List<Orcamento>> ObterTodosParaHistoricoAsync()
        {
            var lista = new List<Orcamento>();

            using (var conn = (MySqlConnection)DbConnectionFactory.CreateConnection())
            {
                await conn.OpenAsync();

                string sql = @"
                    SELECT id, data_criacao, cliente_nome, veiculo_modelo, veiculo_placa, valor_total 
                    FROM orcamento 
                    ORDER BY id DESC";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Orcamento
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            DataCriacao = Convert.ToDateTime(reader["data_criacao"]),
                            ClienteNome = reader["cliente_nome"].ToString(),
                            VeiculoModelo = reader["veiculo_modelo"].ToString(),
                            VeiculoPlaca = reader["veiculo_placa"].ToString(),
                            ValorTotal = Convert.ToDecimal(reader["valor_total"])
                        });
                    }
                }
            }

            return lista;
        }
    }
}