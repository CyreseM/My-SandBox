import { useNavigate, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as Yup from 'yup';
import { MessageSquare } from 'lucide-react';
import { Input, LoadingButton, FormField } from '@/components/ui';
import { withToast } from '@/lib/toast';
import { api } from '@/lib/axios';

const loginSchema = Yup.object({
  username: Yup.string().min(3, 'Min 3 chars').required('Required'),
  password: Yup.string().min(6, 'Min 6 chars').required('Required'),
});

const registerSchema = Yup.object({
  displayName:     Yup.string().min(1).max(64).required('Required'),
  username:        Yup.string().matches(/^[a-zA-Z0-9_]+$/, 'Letters, numbers, _ only').min(3).max(32).required('Required'),
  password:        Yup.string().min(6, 'Min 6 chars').required('Required'),
  confirmPassword: Yup.string().oneOf([Yup.ref('password')], 'Passwords must match').required('Required'),
});

function AuthLayout({ title, subtitle, children, footer }) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-background p-4">
      <div className="w-full max-w-sm space-y-6 animate-fade-in">
        {/* Logo */}
        <div className="text-center space-y-2">
          <div className="inline-flex items-center justify-center h-14 w-14 rounded-2xl bg-primary/10 border border-primary/20 mb-2">
            <MessageSquare className="h-7 w-7 text-primary" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-foreground">{title}</h1>
          <p className="text-sm text-muted-foreground">{subtitle}</p>
        </div>
        <div className="rounded-2xl border border-border bg-card p-6 shadow-lg space-y-4">
          {children}
        </div>
        {footer}
      </div>
    </div>
  );
}

export default function LoginPage() {
  const navigate = useNavigate();
  const { register, handleSubmit, formState: { errors, isSubmitting } } =
    useForm({ resolver: yupResolver(loginSchema) });

  const onSubmit = async (values) => {
    await withToast(() => api.post('/auth/login', values), {
      loading: 'Signing in…', success: 'Welcome back!'
    });
    navigate('/', { replace: true });
  };

  return (
    <AuthLayout
      title="Welcome back"
      subtitle="Sign in to continue to ChatApp"
      footer={
        <p className="text-center text-sm text-muted-foreground">
          No account?{' '}
          <Link to="/register" className="text-primary hover:underline font-medium">Create one</Link>
        </p>
      }
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <FormField label="Username" error={errors.username}>
          <Input {...register('username')} placeholder="your_username" autoFocus />
        </FormField>
        <FormField label="Password" error={errors.password}>
          <Input {...register('password')} type="password" placeholder="••••••••" />
        </FormField>
        <LoadingButton type="submit" loading={isSubmitting} className="w-full">
          Sign In
        </LoadingButton>
      </form>
    </AuthLayout>
  );
}

export function RegisterPage() {
  const navigate = useNavigate();
  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } =
    useForm({ resolver: yupResolver(registerSchema) });

  const onSubmit = async (values) => {
    try {
      await withToast(() => api.post('/auth/register', values), {
        loading: 'Creating account…', success: 'Account created!'
      });
      navigate('/', { replace: true });
    } catch (err) {
      if (err?.response?.data?.field === 'username')
        setError('username', { message: 'Username already taken' });
    }
  };

  return (
    <AuthLayout
      title="Create account"
      subtitle="Join ChatApp today"
      footer={
        <p className="text-center text-sm text-muted-foreground">
          Already have an account?{' '}
          <Link to="/login" className="text-primary hover:underline font-medium">Sign in</Link>
        </p>
      }
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <FormField label="Display Name" error={errors.displayName}>
          <Input {...register('displayName')} placeholder="Jane Doe" autoFocus />
        </FormField>
        <FormField label="Username" error={errors.username}>
          <Input {...register('username')} placeholder="jane_doe" />
        </FormField>
        <FormField label="Password" error={errors.password}>
          <Input {...register('password')} type="password" />
        </FormField>
        <FormField label="Confirm Password" error={errors.confirmPassword}>
          <Input {...register('confirmPassword')} type="password" />
        </FormField>
        <LoadingButton type="submit" loading={isSubmitting} className="w-full">
          Create Account
        </LoadingButton>
      </form>
    </AuthLayout>
  );
}
