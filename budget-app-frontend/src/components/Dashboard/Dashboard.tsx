import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  List,
  ListItem,
  ListItemText,
  Avatar,
  Chip,
  Skeleton,
} from '@mui/material';
import {
  AccountBalance as AccountIcon,
  Receipt as TransactionIcon,
  Group as HouseholdIcon,
  TrendingUp as TrendingUpIcon,
  AttachFile as AttachFileIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { apiService } from '../../services/apiService';
import { Account, Transaction, Household } from '../../types';

const Dashboard: React.FC = () => {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [households, setHouseholds] = useState<Household[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      const [accountsData, transactionsData, householdsData] = await Promise.all([
        apiService.getAccounts(),
        apiService.getTransactions(),
        apiService.getHouseholds(),
      ]);

      setAccounts(accountsData);
      setTransactions(transactionsData.slice(0, 5)); // Show only latest 5
      setHouseholds(householdsData);
    } catch (error) {
      console.error('Error loading dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  const totalBalance = accounts.reduce((sum, account) => sum + account.currentBalance, 0);
  const thisMonthTransactions = transactions.filter(
    (t) => new Date(t.date).getMonth() === new Date().getMonth()
  );
  const thisMonthTotal = thisMonthTransactions.reduce((sum, t) => sum + t.amount, 0);

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('sv-SE', {
      style: 'currency',
      currency: 'SEK',
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('sv-SE');
  };

  if (loading) {
    return (
      <Box>
        <Typography variant="h4" gutterBottom>
          Dashboard
        </Typography>
        <Box display="flex" flexWrap="wrap" gap={2}>
          {[1, 2, 3, 4].map((i) => (
            <Card key={i} sx={{ minWidth: 275, flex: '1 1 200px' }}>
              <CardContent>
                <Skeleton variant="text" width="80%" />
                <Skeleton variant="text" width="60%" />
                <Skeleton variant="rectangular" height={40} />
              </CardContent>
            </Card>
          ))}
        </Box>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>

      {/* Summary Cards */}
      <Box display="flex" flexWrap="wrap" gap={3} sx={{ mb: 4 }}>
        <Card sx={{ minWidth: 275, flex: '1 1 200px' }}>
          <CardContent>
            <Box display="flex" alignItems="center">
              <Avatar sx={{ bgcolor: 'primary.main', mr: 2 }}>
                <AccountIcon />
              </Avatar>
              <Box>
                <Typography color="textSecondary" gutterBottom>
                  Totalt saldo
                </Typography>
                <Typography variant="h6">
                  {formatCurrency(totalBalance)}
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>

        <Card sx={{ minWidth: 275, flex: '1 1 200px' }}>
          <CardContent>
            <Box display="flex" alignItems="center">
              <Avatar sx={{ bgcolor: 'secondary.main', mr: 2 }}>
                <TransactionIcon />
              </Avatar>
              <Box>
                <Typography color="textSecondary" gutterBottom>
                  Denna månad
                </Typography>
                <Typography variant="h6">
                  {formatCurrency(thisMonthTotal)}
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>

        <Card sx={{ minWidth: 275, flex: '1 1 200px' }}>
          <CardContent>
            <Box display="flex" alignItems="center">
              <Avatar sx={{ bgcolor: 'success.main', mr: 2 }}>
                <HouseholdIcon />
              </Avatar>
              <Box>
                <Typography color="textSecondary" gutterBottom>
                  Hushåll
                </Typography>
                <Typography variant="h6">
                  {households.length}
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>

        <Card sx={{ minWidth: 275, flex: '1 1 200px' }}>
          <CardContent>
            <Box display="flex" alignItems="center">
              <Avatar sx={{ bgcolor: 'info.main', mr: 2 }}>
                <TrendingUpIcon />
              </Avatar>
              <Box>
                <Typography color="textSecondary" gutterBottom>
                  Antal konton
                </Typography>
                <Typography variant="h6">
                  {accounts.length}
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>
      </Box>

      <Box display="flex" flexWrap="wrap" gap={3}>
        {/* Recent Transactions */}
        <Card sx={{ minWidth: 300, flex: '1 1 300px' }}>
          <CardContent>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
              <Typography variant="h6">Senaste transaktioner</Typography>
              <Button
                size="small"
                onClick={() => navigate('/transactions')}
              >
                Visa alla
              </Button>
            </Box>
            {transactions.length === 0 ? (
              <Typography color="textSecondary">
                Inga transaktioner än. <Button onClick={() => navigate('/import')}>Importera CSV</Button>
              </Typography>
            ) : (
              <List dense>
                {transactions.map((transaction) => (
                  <ListItem key={transaction.id} divider>
                    <ListItemText
                      primary={transaction.description}
                      secondary={formatDate(transaction.date)}
                    />
                    <Box textAlign="right" display="flex" alignItems="center" gap={1}>
                      <Box>
                        <Typography
                          variant="body2"
                          color={transaction.amount >= 0 ? 'success.main' : 'error.main'}
                        >
                          {formatCurrency(transaction.amount)}
                        </Typography>
                        {transaction.category && (
                          <Chip
                            label={transaction.category.name}
                            size="small"
                            sx={{ mt: 0.5 }}
                          />
                        )}
                      </Box>
                      {transaction.attachments && transaction.attachments.length > 0 && (
                        <AttachFileIcon fontSize="small" color="action" />
                      )}
                    </Box>
                  </ListItem>
                ))}
              </List>
            )}
          </CardContent>
        </Card>

        {/* Quick Actions */}
        <Card sx={{ minWidth: 300, flex: '1 1 300px' }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Snabbåtgärder
            </Typography>
            <Box display="flex" flexDirection="column" gap={2}>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<AccountIcon />}
                onClick={() => navigate('/accounts')}
              >
                Hantera konton
              </Button>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<TransactionIcon />}
                onClick={() => navigate('/transactions')}
              >
                Visa transaktioner
              </Button>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<HouseholdIcon />}
                onClick={() => navigate('/households')}
              >
                Hantera hushåll
              </Button>
              <Button
                fullWidth
                variant="contained"
                onClick={() => navigate('/import')}
              >
                Importera CSV
              </Button>
            </Box>
          </CardContent>
        </Card>
      </Box>
    </Box>
  );
};

export default Dashboard;