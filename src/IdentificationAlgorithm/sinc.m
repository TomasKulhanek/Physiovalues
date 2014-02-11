function y = sinc(x)
y = zeros(size(x));
epsil = 1e-6;
y(abs(x)<epsil) = x(abs(x)<epsil);
y(abs(x)>=epsil) = sin(x(abs(x)>=epsil))./x(abs(x)>=epsil);
%y = sin(x)./x;