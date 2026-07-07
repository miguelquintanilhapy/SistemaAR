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
        /// Salva o orçamento e seus itens em uma única transação. Insere um orçamento novo (Id == 0)
        /// ou atualiza um orçamento existente, substituindo integralmente os itens vinculados.
        /// </summary>
        public async Task<int> SalvarCompletoAsync(Orcamento orcamento, List<ItemOrcamento> itens)
        {
            using (var conn = (MySqlConnection)DbConnectionFactory.CreateConnection())
            {
                await conn.OpenAsync();

                using (var transacao = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        int orcamentoId = orcamento.Id > 0
                            ? await AtualizarCabecalhoAsync(conn, transacao, orcamento)
                            : await InserirCabecalhoAsync(conn, transacao, orcamento);

                        string sqlDeleteItens = "DELETE FROM item_orcamento WHERE orcamento_id = @orcamentoId;";
                        using (var cmdDelete = new MySqlCommand(sqlDeleteItens, conn, transacao))
                        {
                            cmdDelete.Parameters.AddWithValue("@orcamentoId", orcamentoId);
                            await cmdDelete.ExecuteNonQueryAsync();
                        }

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

                        await transacao.CommitAsync();
                        return orcamentoId;
                    }
                    catch (Exception)
                    {
                        await transacao.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        private async Task<int> InserirCabecalhoAsync(MySqlConnection conn, MySqlTransaction transacao, Orcamento orcamento)
        {
            string sql = @"
                INSERT INTO orcamento
                (cliente_nome, cliente_telefone, veiculo_marca, veiculo_modelo,
                 veiculo_placa, veiculo_cor, veiculo_ano, garantia_dias,
                 observacoes, valor_total)
                VALUES
                (@nome, @tel, @marca, @modelo, @placa, @cor, @ano, @garantia, @obs, @total);
                SELECT LAST_INSERT_ID();";

            using (var cmd = new MySqlCommand(sql, conn, transacao))
            {
                PreencherParametrosCabecalho(cmd, orcamento);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        private async Task<int> AtualizarCabecalhoAsync(MySqlConnection conn, MySqlTransaction transacao, Orcamento orcamento)
        {
            string sql = @"
                UPDATE orcamento SET
                    cliente_nome = @nome, cliente_telefone = @tel, veiculo_marca = @marca,
                    veiculo_modelo = @modelo, veiculo_placa = @placa, veiculo_cor = @cor,
                    veiculo_ano = @ano, garantia_dias = @garantia, observacoes = @obs, valor_total = @total
                WHERE id = @id;";

            using (var cmd = new MySqlCommand(sql, conn, transacao))
            {
                PreencherParametrosCabecalho(cmd, orcamento);
                cmd.Parameters.AddWithValue("@id", orcamento.Id);
                await cmd.ExecuteNonQueryAsync();
                return orcamento.Id;
            }
        }

        private void PreencherParametrosCabecalho(MySqlCommand cmd, Orcamento orcamento)
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
                    SELECT id, data_criacao, cliente_nome, cliente_telefone, veiculo_marca, veiculo_modelo, veiculo_placa, valor_total
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
                            ClienteTelefone = reader["cliente_telefone"] as string,
                            VeiculoMarca = reader["veiculo_marca"] as string,
                            VeiculoModelo = reader["veiculo_modelo"].ToString(),
                            VeiculoPlaca = reader["veiculo_placa"].ToString(),
                            ValorTotal = Convert.ToDecimal(reader["valor_total"])
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Busca um orçamento completo (cabeçalho + itens) para reabertura na tela de edição/impressão.
        /// </summary>
        public async Task<(Orcamento orcamento, List<ItemOrcamento> itens)> ObterPorIdAsync(int id)
        {
            using (var conn = (MySqlConnection)DbConnectionFactory.CreateConnection())
            {
                await conn.OpenAsync();

                Orcamento orcamento = null;
                string sqlCabecalho = @"
                    SELECT id, data_criacao, cliente_nome, cliente_telefone, veiculo_marca, veiculo_modelo,
                           veiculo_placa, veiculo_cor, veiculo_ano, garantia_dias, observacoes, valor_total
                    FROM orcamento
                    WHERE id = @id;";

                using (var cmd = new MySqlCommand(sqlCabecalho, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            orcamento = new Orcamento
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                DataCriacao = Convert.ToDateTime(reader["data_criacao"]),
                                ClienteNome = reader["cliente_nome"].ToString(),
                                ClienteTelefone = reader["cliente_telefone"] as string,
                                VeiculoMarca = reader["veiculo_marca"] as string,
                                VeiculoModelo = reader["veiculo_modelo"] as string,
                                VeiculoPlaca = reader["veiculo_placa"] as string,
                                VeiculoCor = reader["veiculo_cor"] as string,
                                VeiculoAno = reader["veiculo_ano"] is DBNull ? 0 : Convert.ToInt32(reader["veiculo_ano"]),
                                GarantiaDias = Convert.ToInt32(reader["garantia_dias"]),
                                Observacoes = reader["observacoes"] as string,
                                ValorTotal = Convert.ToDecimal(reader["valor_total"])
                            };
                        }
                    }
                }

                if (orcamento == null) return (null, new List<ItemOrcamento>());

                var itens = new List<ItemOrcamento>();
                string sqlItens = "SELECT id, orcamento_id, descricao, valor_unitario FROM item_orcamento WHERE orcamento_id = @id ORDER BY id;";

                using (var cmd = new MySqlCommand(sqlItens, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            itens.Add(new ItemOrcamento
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                OrcamentoId = Convert.ToInt32(reader["orcamento_id"]),
                                Descricao = reader["descricao"].ToString(),
                                ValorUnitario = Convert.ToDecimal(reader["valor_unitario"])
                            });
                        }
                    }
                }

                return (orcamento, itens);
            }
        }
    }
}
