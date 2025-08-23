import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  ListItemButton,
  Avatar,
  Alert,
  CircularProgress,
  IconButton,
  Tooltip
} from '@mui/material';
import {
  AccountBalance as BankIcon,
  Add as AddIcon,
  Sync as SyncIcon,
  Delete as DeleteIcon,
  Link as LinkIcon,
  Download as ImportIcon
} from '@mui/icons-material';
import { apiService } from '../../services/apiService';
import {
  BankConnection,
  Bank,
  ExternalAccount,
  Account,
  ConnectionStatus
} from '../../types';

interface BankConnectionsProps {
  accounts: Account[];
  onRefresh?: () => void;
}

const BankConnections: React.FC<BankConnectionsProps> = ({ accounts, onRefresh }) => {
  const [connections, setConnections] = useState<BankConnection[]>([]);
  const [banks, setBanks] = useState<Bank[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [openBankDialog, setOpenBankDialog] = useState(false);
  const [selectedConnection, setSelectedConnection] = useState<BankConnection | null>(null);
  const [externalAccounts, setExternalAccounts] = useState<ExternalAccount[]>([]);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const [connectionsData, banksData] = await Promise.all([
        apiService.getBankConnections(),
        apiService.getAvailableBanks()
      ]);
      setConnections(connectionsData);
      setBanks(banksData);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load bank connections');
    } finally {
      setLoading(false);
    }
  };

  const handleConnectBank = async (bankId: string) => {
    try {
      const redirectUrl = `${window.location.origin}/psd2/callback`;
      const result = await apiService.createBankConnection({
        bankId,
        redirectUrl
      });

      if (result.success && result.authorizationUrl) {
        // In a real implementation, this would redirect to the bank's authorization page
        // For demo purposes, we'll simulate completion
        if (result.connectionId) {
          setTimeout(async () => {
            try {
              await apiService.completeBankConnection(result.connectionId!, 'demo_auth_code');
              await loadData();
              setOpenBankDialog(false);
            } catch (err: any) {
              setError(err.response?.data?.error || 'Failed to complete bank connection');
            }
          }, 2000);
        }
      } else {
        setError(result.error || 'Failed to create bank connection');
      }
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to connect to bank');
    }
  };

  const handleSyncConnection = async (connectionId: number) => {
    try {
      await apiService.syncBankConnection(connectionId);
      await loadData();
      onRefresh?.();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to sync bank connection');
    }
  };

  const handleDisconnectBank = async (connectionId: number) => {
    if (window.confirm('Are you sure you want to disconnect this bank? This will remove all external account data.')) {
      try {
        await apiService.disconnectBank(connectionId);
        await loadData();
      } catch (err: any) {
        setError(err.response?.data?.error || 'Failed to disconnect bank');
      }
    }
  };

  const loadExternalAccounts = async (connection: BankConnection) => {
    try {
      const accountsData = await apiService.getExternalAccounts(connection.id);
      setExternalAccounts(accountsData);
      setSelectedConnection(connection);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load external accounts');
    }
  };

  const getStatusColor = (status: ConnectionStatus) => {
    switch (status) {
      case ConnectionStatus.Active:
        return 'success';
      case ConnectionStatus.Pending:
        return 'warning';
      case ConnectionStatus.ConsentExpired:
        return 'error';
      case ConnectionStatus.Error:
        return 'error';
      case ConnectionStatus.Disconnected:
        return 'default';
      default:
        return 'default';
    }
  };

  const getStatusText = (status: ConnectionStatus) => {
    switch (status) {
      case ConnectionStatus.Active:
        return 'Active';
      case ConnectionStatus.Pending:
        return 'Pending';
      case ConnectionStatus.ConsentExpired:
        return 'Consent Expired';
      case ConnectionStatus.Error:
        return 'Error';
      case ConnectionStatus.Disconnected:
        return 'Disconnected';
      default:
        return 'Unknown';
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h5" component="h2">
          Bank Connections
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => setOpenBankDialog(true)}
        >
          Connect Bank
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Box display="flex" flexWrap="wrap" gap={3}>
        {connections.map((connection) => (
          <Box key={connection.id} sx={{ minWidth: '300px', flex: '1 1 300px', maxWidth: '400px' }}>
            <Card>
              <CardContent>
                <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={2}>
                  <Box display="flex" alignItems="center">
                    <BankIcon sx={{ mr: 1, color: 'primary.main' }} />
                    <Typography variant="h6">{connection.bankName}</Typography>
                  </Box>
                  <Chip
                    label={getStatusText(connection.status)}
                    color={getStatusColor(connection.status)}
                    size="small"
                  />
                </Box>

                <Typography variant="body2" color="text.secondary" gutterBottom>
                  {connection.accountCount} accounts connected
                </Typography>

                {connection.lastSyncAt && (
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Last synced: {new Date(connection.lastSyncAt).toLocaleDateString()}
                  </Typography>
                )}

                {connection.consentExpiresAt && (
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Consent expires: {new Date(connection.consentExpiresAt).toLocaleDateString()}
                  </Typography>
                )}

                {connection.errorMessage && (
                  <Alert severity="error" sx={{ mt: 1, mb: 1 }}>
                    {connection.errorMessage}
                  </Alert>
                )}

                <Box display="flex" justifyContent="space-between" mt={2}>
                  <Box>
                    <Tooltip title="View Accounts">
                      <IconButton
                        size="small"
                        onClick={() => loadExternalAccounts(connection)}
                        disabled={connection.status !== ConnectionStatus.Active}
                      >
                        <LinkIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Sync">
                      <IconButton
                        size="small"
                        onClick={() => handleSyncConnection(connection.id)}
                        disabled={connection.status !== ConnectionStatus.Active}
                      >
                        <SyncIcon />
                      </IconButton>
                    </Tooltip>
                  </Box>
                  <Tooltip title="Disconnect">
                    <IconButton
                      size="small"
                      color="error"
                      onClick={() => handleDisconnectBank(connection.id)}
                    >
                      <DeleteIcon />
                    </IconButton>
                  </Tooltip>
                </Box>
              </CardContent>
            </Card>
          </Box>
        ))}
      </Box>

      {connections.length === 0 && (
        <Card>
          <CardContent>
            <Box textAlign="center" py={4}>
              <BankIcon sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No bank connections
              </Typography>
              <Typography variant="body2" color="text.secondary" mb={3}>
                Connect your bank to automatically import transactions and account balances.
              </Typography>
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={() => setOpenBankDialog(true)}
              >
                Connect Your First Bank
              </Button>
            </Box>
          </CardContent>
        </Card>
      )}

      {/* Bank Selection Dialog */}
      <Dialog
        open={openBankDialog}
        onClose={() => setOpenBankDialog(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Select Bank to Connect</DialogTitle>
        <DialogContent>
          <List>
            {banks.map((bank) => (
              <ListItem key={bank.id}>
                <ListItemButton onClick={() => handleConnectBank(bank.id)}>
                  <ListItemAvatar>
                    <Avatar>
                      <BankIcon />
                    </Avatar>
                  </ListItemAvatar>
                  <ListItemText
                    primary={bank.name}
                    secondary="Connect securely via Open Banking"
                  />
                </ListItemButton>
              </ListItem>
            ))}
          </List>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenBankDialog(false)}>Cancel</Button>
        </DialogActions>
      </Dialog>

      {/* External Accounts Dialog */}
      <Dialog
        open={!!selectedConnection}
        onClose={() => setSelectedConnection(null)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          {selectedConnection?.bankName} Accounts
        </DialogTitle>
        <DialogContent>
          <List>
            {externalAccounts.map((account) => (
              <ListItem key={account.id}>
                <ListItemAvatar>
                  <Avatar>
                    <BankIcon />
                  </Avatar>
                </ListItemAvatar>
                <ListItemText
                  primary={account.accountName}
                  secondary={
                    <Box>
                      <Typography variant="body2">
                        {account.accountNumber} • {account.currency} {account.currentBalance.toLocaleString()}
                      </Typography>
                      {account.linkedAccountName && (
                        <Typography variant="body2" color="primary">
                          Linked to: {account.linkedAccountName}
                        </Typography>
                      )}
                    </Box>
                  }
                />
                <Box>
                  {!account.linkedAccountId && accounts.length > 0 && (
                    <Button size="small" startIcon={<LinkIcon />}>
                      Link Account
                    </Button>
                  )}
                  <Button size="small" startIcon={<ImportIcon />}>
                    Import Transactions
                  </Button>
                </Box>
              </ListItem>
            ))}
          </List>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSelectedConnection(null)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default BankConnections;